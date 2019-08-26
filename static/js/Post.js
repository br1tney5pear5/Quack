
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

    postDOM.innerHTML += 
        '<div> \
           <form action="/Account/AddComment" method="post" \
                onsubmit="hideCommentBox(' + post.id + ')"> \
            <div class="comment-input-frame"> \
                <div class="text-input-frame"> \
                <input type="hidden" id="returnUrl" name="returnUrl" value="' +
        (location.href) + '"> \
                <input type="hidden" id="postID" name="postID" value="' + post.id + '"> \
                <textarea type="text" id="text" name="text"\
                    class="text-input" placeholder="Write something..."></textarea> </div> \
                <div class="text-input-bottom"> \
                <button class="btn post-btn" type="submit">Comment</button> \
                </div> \
            </div> \
            <div style="clear:both"> </div> \
           </form> \
        </div> ';

    postDOM.innerHTML += '<button class="btn comment-input-toggle" style="display:block" onclick=" \
        showCommentBox( ' + post.id + ')">Write Comment</button> \
                          <button class="btn comment-input-toggle" style="display:none" onclick=" \
        hideCommentBox( ' + post.id + ')">Give up</button> \
    </div>';

    return postDOM;
}

function showCommentBox(postID) {
    var post = document.getElementById("post" + postID.toString());
    post.getElementsByClassName("comment-input-frame")[0].style.display = "block";
    post.getElementsByClassName("btn comment-input-toggle")[0].style.display = "none";
    post.getElementsByClassName("btn comment-input-toggle")[1].style.display = "block";
}
function hideCommentBox(postID) {
    var post = document.getElementById("post" + postID.toString());
    post.getElementsByClassName("comment-input-frame")[0].style.display = "none";
    post.getElementsByClassName("btn comment-input-toggle")[0].style.display = "block";
    post.getElementsByClassName("btn comment-input-toggle")[1].style.display = "none";
}



function renderComment(comment){

    var commentDOM = document.createElement('div');
    commentDOM.className += "post-comment-frame";
    commentDOM.id = "comment" + comment.id.toString();

    commentDOM.innerHTML = '<div class="comment-header"> \
        <img class="profile-picture" \
    src="' + comment.avatarUrl + '"> \
        <a class="profile-name" href="/Account/User?ID=' +
            comment.authorID + '"> ' + comment.username + '</a> \
        <span class="post-date">' + sqlDateToReadible(comment.datePublished) + '</span> \
    </div> \
        <div class="comment-content"> ' + comment.text + '</div>';
    return commentDOM;
}

const _postRequest = (url, args, onSuccess) => $.ajax({
        type: 'POST', url: url, data: args, dataType: 'html', success: onSuccess,
        error: function() { console.error("request error"); }
    });

const _getArray = (url, callback, args) =>
    _postRequest(url, args, function(data) {
        var stagedArray = JSON.parse(data);
        if(Array.isArray(stagedArray)){
            callback(stagedArray);
        } else{
            console.error("server error");
            callback([]);
        }
    });

const getPosts = (callback, args) =>
    _getArray("/Account/GetPosts", callback, args);

const getPostsAfter = (callback, args) =>
    _getArray("/Account/GetPostsAfter", callback, args);

const getPostsBefore = (callback, args) =>
    _getArray("/Account/GetPostsBefore", callback, args);

const getPostComments = (callback, args) =>
    _getArray("/Account/GetPostComments", callback, args);

// ...yes, I really like lambdas

function prependPosts(posts, to){
    for(var i=posts.length-1; i >= 0; i--){
        var postDOM = renderPost(posts[i]);
        to.prepend(postDOM);
        postRefArr.unshift(postDOM);
    }
}

function appendPosts(posts, to){
    posts.forEach(post => {
        var postDOM = renderPost(post);
        to.append(postDOM);
        postRefArr.push(postDOM);
    });
}



