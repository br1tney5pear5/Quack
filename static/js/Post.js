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


