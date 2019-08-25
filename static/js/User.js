var postSkip = 0;
var pageSize = 5;
var lastLoadedPostID = -1;
var newestLoadedPostID = -1;
var postRefArr = new Array();

function loadCommentsForPost(post) {
    $.ajax({
        type: 'POST',
        url: '/Account/GetPostComments',
        data: {
            "ID": parseInt(post.id.split("post")[1])
        },
        dataType: 'html',
        success: function(data) {
            var comments = JSON.parse(data);
            if(Array.isArray(comments)){
                var newcoms = "";
                var p = post.getElementsByClassName("post-comments-frame")[0];
                // Programming Gods, I am really sorry for the following
                comments.forEach(c => {
                    if(![].slice.call(p.childNodes).some(comDOM => {
                        if(parseInt(comDOM.id.split("comment")[1]) == c.id) return true;
                    })) {
                        p.appendChild(renderComment(c));
                    }
                });
            } else {
                console.error("server error");
            }
        }
    });
}

function reloadComments(){ // realoads only few first postsss
    // for(var i=postRefArr.length-1;
    //     i >= 0 && i > postRefArr.length-20;
    //     i--){
    for(var i=0;
        i < postRefArr.length;
        i++){
        var p = postRefArr[i];
        loadCommentsForPost(p);
    }
}

function prependPosts(posts){
    for(var i=posts.length-1; i >= 0; i--){
        var post = posts[i];
        var postDOM = renderPost(post);
        $("#post-view").prepend(postDOM);
        postRefArr.unshift(postDOM);

    }
}

function appendPosts(posts){
    posts.forEach(post => {
        var postDOM = renderPost(post);
        $("#post-view").append(postDOM);
        postRefArr.push(postDOM);
    });
}

function initialLoadPosts(){
    $.ajax({
        type: 'POST',
        url: '/Account/GetUserPosts',
        data: {
            "userID" : userId,
            "count": pageSize
        },
        dataType: 'html',
        success: function(data) {
            var posts = JSON.parse(data);
            if(Array.isArray(posts)){
                if(posts.length == 0) return;

                newestLoadedPostID = parseInt(posts[0].id);

                lastLoadedPostID = parseInt(posts[posts.length -1].id);

                $("#post-view").empty();
                appendPosts(posts);
            } else {
                console.error("server error");
            }
        }
    });
}

function loadNewPosts(){
    if(newestLoadedPostID < 0){
        initialLoadPosts();
    }else {
        $.ajax({
            type: 'POST',
            url: '/Account/GetUserPostsAfter',
            data: {"ID": newestLoadedPostID,
                   "userID" : userId,
                   "count": pageSize
                  },
            dataType: 'html',
            error: function(data) {
                console.error("failed to fetch data");
            },
            success: function(data) {
                var posts = JSON.parse(data);
                if(Array.isArray(posts)){
                    if(posts.length == 0) return;

                    newestLoadedPostID = parseInt(posts[0].id);
                    prependPosts(posts);
                    console.log("intial load");
                } else {
                    console.error("server error");
                }
            }
        });
    }
}

function loadMorePosts(){
    if(lastLoadedPostID < 0){
        initialLoadPosts();
    }else {
        $.ajax({
            type: 'POST',
            url: '/Account/GetUserPostsBefore',
            data: {"ID": lastLoadedPostID,
                   "userID" : userId,
                   "count": pageSize
                  },
            dataType: 'html',
            success: function(data) {
                var posts = JSON.parse(data);
                if(Array.isArray(posts)){
                    if(posts.length == 0) return;

                    lastLoadedPostID = posts[posts.length -1].id;
                    appendPosts(posts);
                }
            }
        });
    }
}

$( document ).ready(function() {
    initialLoadPosts();


    setInterval(function() {
        var triggerTop   = $('#top-trigger').offset().top;
        var windowTop    = $( window ).scrollTop();
        var windowBottom = windowTop + $( window ).height() * 2; 
        if(triggerTop > windowTop && triggerTop < windowBottom) {
            reloadComments();
        }
    }, 4000);
});

$( document ).scroll(function(e) {
    var triggerBottom   = $('#bottom-trigger').offset().top;
    var windowTop    = $( window ).scrollTop();
    var windowBottom = windowTop + $( window ).height(); 
    if(triggerBottom > windowTop && triggerBottom < windowBottom) {
        loadMorePosts();
    }
});
