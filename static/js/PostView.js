var postSkip = 0;
var garbageCollectionThres = 100;
var pageSize = 5;
var lastLoadedPostID = -1;
var newestLoadedPostID = -1;
var ofUserID = null;
var forUserID = null;


function loadCommentsForPost(post) {
    getPostComments(function(comments) {
        var newcoms = "";
        var p = post.getElementsByClassName("post-comments-frame")[0];
        // Programming Gods, please excuse me for the following
        comments.forEach(c => {
            if(![].slice.call(p.childNodes).some(comDOM =>
                parseInt(comDOM.id.split("comment")[1]) == c.id ? true : false)
            ) p.appendChild(renderComment(c));
        });
    }, {"ID": parseInt(post.id.split("post")[1])});
}

function reloadComments(){ 
    for(var i=0; i < postRefArr.length; i++){
        var p = postRefArr[i];
        loadCommentsForPost(p);
    }
}

function initialLoadPosts(){
    getPosts(function(posts){
        if(posts.length == 0) return;

        newestLoadedPostID = parseInt(posts[0].id);
        lastLoadedPostID = parseInt(posts[posts.length -1].id);

        $("#post-view").empty();

        appendPosts(posts, $("#post-view"));
        console.log("intial load");
    }, {"count":pageSize,
        "ofUserID" : ofUserID,
        "forUserID" : forUserID
       });
}

function loadNewPosts(){
    if(newestLoadedPostID < 0){
        initialLoadPosts();
    } else {
        getPostsAfter(function(posts){
            if(posts.length == 0) return;
            if(newestLoadedPostID == parseInt(posts[0].id)) return;

            newestLoadedPostID = parseInt(posts[0].id);
            prependPosts(posts, $("#post-view"));
        }, {"ID" : newestLoadedPostID,
            "maxcount" : pageSize,
            "ofUserID" : ofUserID,
            "forUserID" : forUserID
           });
    }
}

function loadMorePosts(){
    if(lastLoadedPostID < 0){
        initialLoadPosts();
    }else {
        getPostsBefore(function(posts) {
            if(posts.length == 0) return;

            lastLoadedPostID = posts[posts.length -1].id;
            appendPosts(posts, $("#post-view"));
        }, {"ID": lastLoadedPostID,
            "count": pageSize,
            "ofUserID" : ofUserID,
            "forUserID" : forUserID
           });
    }
}

function isOnTop() {
    var triggerTop   = $('#top-trigger').offset().top;
    var windowTop    = $( window ).scrollTop();
    var windowBottom = windowTop + $( window ).height() * 2;
    return (triggerTop > windowTop && triggerTop < windowBottom);
}

function isOnBottom() {
    var triggerBottom   = $('#bottom-trigger').offset().top;
    var windowTop    = $( window ).scrollTop();
    var windowBottom = windowTop + $( window ).height(); 
    return (triggerBottom > windowTop && triggerBottom < windowBottom);
}

function collectGarbage(){
    if(postRefArr.length > garbageCollectionThres) {
        for(var i=postRefArr.length-1; i >= garbageCollectionThres; i--) {
            postRefArr[i].remove();
        }
        postRefArr = postRefArr.slice(0, garbageCollectionThres);
        console.log("garbage collection");
        console.log(postRefArr.length);
    }
}

$( document ).ready(function() {
    initialLoadPosts();

    setInterval(function() {
        if(isOnTop()){
            loadNewPosts();
            collectGarbage();
            reloadComments();
            console.log("update");
        }
    }, 1000);
});

$( document ).scroll( e => { if(isOnBottom()) loadMorePosts(); });
