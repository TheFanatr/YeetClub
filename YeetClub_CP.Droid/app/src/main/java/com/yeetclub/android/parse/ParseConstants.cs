using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.parse
{

/// <summary>
/// Created by @santafebound on 2015-11-07.
/// </summary>
public class ParseConstants {

    // Type names
    public const string CLASS_NOTIFICATIONS = "Notification";
    public const string CLASS_YEET = "Yeet";
    public const string CLASS_COMMENT = "Comment";
    public const string CLASS_LIKE = "Like";
    public const string CLASS_POLL = "Poll";
    public const string CLASS_GROUP = "Group";

    // Generic
    public const string KEY_OBJECT_ID = "objectId";

    // Time-related
    public const string KEY_CREATED_AT = "createdAt";
    public const string KEY_LAST_REPLY_UPDATED_AT = "lastReplyUpdatedAt";

    // User-related
    public const string KEY_GROUP_ID = "groupId";
    public const string KEY_USERNAME = "username";
    public const string KEY_AUTHOR_FULL_NAME = "name";
    public const string KEY_SENDER_AUTHOR_POINTER = "author";
    public const string KEY_PROFILE_PICTURE = "profilePicture";
    public const string KEY_CURRENT_GROUP = "currentGroup";
    public const string KEY_USER_BIO = "bio";
    public const string KEY_VERIFIED = "verified";

    // Group-related
    public const string KEY_GROUP_NAME = "name";
    public const string KEY_GROUP_DESCRIPTION = "description";
    public const string KEY_MY_GROUPS = "myGroups";
    public const string KEY_GROUP_ADMIN_LIST = "admin";
    public const string KEY_GROUP_SECRET_KEY = "secretKey";
    public const string KEY_GROUP_PRIVATE = "private";

    // Yeet-related
    public const string KEY_REPLY_COUNT = "replyCount";
    public const string KEY_LIKED_BY = "likedBy";
    public const string KEY_COMMENT_OBJECT_ID = "commentObjectId";
    public const string KEY_RANT_ID = "rantId";
    public const string KEY_LIKE_COUNT = "likeCount";

    // Poll-related
    public const string KEY_POLL_OBJECT = "pollObject";
    public const string KEY_POLL_OPTION1 = "option1";
    public const string KEY_POLL_OPTION2 = "option2";
    public const string KEY_POLL_OPTION3 = "option3";
    public const string KEY_POLL_OPTION4 = "option4";
    public const string KEY_POLL_VALUE1 = "value1";
    public const string KEY_POLL_VALUE2 = "value2";
    public const string KEY_POLL_VALUE3 = "value3";
    public const string KEY_POLL_VALUE4 = "value4";
    public const string KEY_POLL_VOTED_BY ="votedBy";

    // Notification-related
    public const string KEY_NOTIFICATION_TYPE = "notificationType";
    public const string TYPE_LIKE = "typeLike";
    public const string TYPE_COMMENT = "typeComment";
    public const string KEY_READ_STATE = "read";
    public const string KEY_SENDER_ID = "senderId";
    public const string KEY_RECIPIENT_ID = "recipientId";

    // Misc
    public const string KEY_NOTIFICATION_TEXT = "notificationText";
    public const string KEY_COMMENT_TEXT = "comment";
    public const string KEY_NOTIFICATION_BODY = "notificationBody";
    public const string KEY_SENDER_NAME = "senderName";
    public const string KEY_SENDER_FULL_NAME = "senderFullName";
    public const string KEY_SENDER_POST_POINTER = "post";
    public const string KEY_SENDER_PARSE_OBJECT_ID = "senderParseObjectId";
    public const string KEY_SENDER_PROFILE_PICTURE = "senderProfilePicture";

}
}
