var postSkip = 0;
var pageSize = 5;
var lastLoadedPostID = -1;
var newestLoadedPostID = -1;
var postRefArr = new Array();

function sqlDateToReadible(date) {
    var temp = new Date(date);
    return temp.getHours() + ':' + temp.getMinutes() + ' ' + temp.toDateString();
}


function renderPost(post) {
    var postDOM = document.createElement('div');
    postDOM.className += "post-container";
    postDOM.id = "post" + post.id.toString();

    postDOM.innerHTML = '<div class="main-post-frame"> \
        <div class="post-header"> \
            <img class="profile-picture" \
                src="' + post.avatarUrl + '"> \
            <a class="profile-name" href="/Account/User?ID=' +
    post.authorID + '"> ' + post.username + '</a> \
            <span class="post-date">' + sqlDateToReadible(post.datePublished) + '</span> \
        </div> \
        <div class="post-content">' + post.content.text +
        '</div> \
    </div>';

    postCommentsDOM = document.createElement('div');
    postCommentsDOM.className += 'post-comments-frame';

    post.comments.forEach(c => postCommentsDOM.appendChild( renderComment(c) ));

    postDOM.appendChild(postCommentsDOM);

    postDOM.innerHTML += '<button class="btn load-comments" onclick="loadCommentsForPost( \
        document.getElementById( "post' + post.id.toString() + '") \
        )">Load comments</button> \
    </div>';

    return postDOM;
}
function renderComment(comment){
    var commentDOM = document.createElement('div');
    commentDOM.className += "post-comment-frame";
    commentDOM.id = "comment" + comment.id.toString();

    console.log(comment);
    commentDOM.innerHTML = '<div class="comment-header"> \
        <img class="profile-picture" \
    src="' + comment.avatarUrl + '"> \
        <a class="profile-name" href="' +
        comment.authorID + '">' + comment.username + '</a> \
        <span class="post-date">' + sqlDateToReadible(comment.datePublished) + '</span> \
    </div> \
        <div class="comment-content"> ' + comment.text + '</div>';
    return commentDOM;
}

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
        url: '/Account/GetPosts',
        data: {
            "skip": 0,
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
            url: '/Account/GetPostsAfter',
            data: {"ID": newestLoadedPostID,
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
            url: '/Account/GetPostsBefore',
            data: {"ID": lastLoadedPostID,
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
            loadNewPosts();
            reloadComments();
            console.log("update");
        }
    }, 1000);
});

$( document ).scroll(function(e) {
    var triggerBottom   = $('#bottom-trigger').offset().top;
    var windowTop    = $( window ).scrollTop();
    var windowBottom = windowTop + $( window ).height(); 
    if(triggerBottom > windowTop && triggerBottom < windowBottom) {
        loadMorePosts();
    }
});
