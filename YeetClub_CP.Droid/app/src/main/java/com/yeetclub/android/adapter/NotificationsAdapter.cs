using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.adapter
{




/// <summary>
/// Created by @santafebound on 2015-11-07.
/// </summary>
public class NotificationsAdapter : RecyclerView.Adapter<RecyclerView.ViewHolder> {

    private const int LIKE = 0, COMMENT = 1;

    protected Context mContext;
    protected List<ParseObject> mNotifications;
    private NotificationsAdapter adapter;

    public NotificationsAdapter(Context context, List<ParseObject> yeets) {
        base();

        this.mNotifications = yeets;
        this.mContext = context;
        this.adapter = this;
    }

    private void launchCommentFromNotification(ParseObject notifications) {

        // We retrieve the permanent objectId of the Yeet
        string userId = string.valueOf(notifications.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId());
        string commentId = string.valueOf(notifications.getString(ParseConstants.KEY_COMMENT_OBJECT_ID));

        // We use the generated commentId to launch the comment activity so that we can populate it with relevant messages
        startCommentActivity(commentId, userId);

    }

    private void startCommentActivity(string commentId, string userId) {
        /// <summary>
        /// If the previously generated commentId is empty, we return nothing. This probably only occurs in the rare instance that the comment was deleted
        /// from the database.
        /// </summary>
        if (commentId == null || commentId.IsEmpty()) {
            return;
        }

        // Here we launch a generic commenty activity class...
        Intent intent = new Intent(mContext, CommentActivity.class);
        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        // ...and send along some information so that we can populate it with the relevant comments.
        intent.putExtra(ParseConstants.KEY_OBJECT_ID, commentId);
        intent.putExtra(ParseConstants.KEY_SENDER_ID, userId);
        mContext.startActivity(intent);
    }

    /// <summary>
    /// @param notifications A list derived from the main "Yeet" ParseObject (Yeet), from which also user information may be obtained via the _User pointer "author".
    /// </summary>
    private void retrievePointerObjectId(ParseObject notifications) {
        // We want to retrieve the permanent user objectId from the author of the Yeet so that we can always launch the user's profile, even if the author changes their username in the future.
        string userId = string.valueOf(notifications.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId());

        // We use the generated userId to launch the user profile depending on whether we arrive to the profile as ourselves or are visiting externally from another feed or Yeet
        startGalleryActivity(userId);
    }

    public void startGalleryActivity(string userId) {
        // If the previously generated userId is empty, we return nothing. This probably only occurs in the rare instance that the author was deleted from the database.
        if (userId == null || userId.IsEmpty()) {
            return;
        }

        // Here we launch a generic user profile class...
        Intent intent = new Intent(mContext, UserProfileActivity.class);
        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        // ...and send along some information so that we can populate it with the relevant user, i.e. either ourselves or another author if visiting from another feed or Yeet.
        intent.putExtra(ParseConstants.KEY_OBJECT_ID, userId);
        mContext.startActivity(intent);
    }

    override public RecyclerView.ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {

        RecyclerView.ViewHolder viewHolder;
        LayoutInflater inflater = LayoutInflater.from(parent.getContext());

        switch (viewType) {
            case LIKE:
                View v1 = inflater.inflate(R.layout.notifications_listview_item, parent, false);
                viewHolder = new ViewHolder(v1);
                break;
            case COMMENT:
                View v2 = inflater.inflate(R.layout.yeet_listview_item, parent, false);
                viewHolder = new ViewHolder2(v2);
                break;
            default:
                View v = inflater.inflate(android.R.layout.simple_list_item_1, parent, false);
                viewHolder = new RecyclerViewSimpleTextViewHolder(v);
                break;
        }
        return viewHolder;
    }

    private void configureDefaultViewHolder(RecyclerViewSimpleTextViewHolder vh, int position) {
        vh.getLabel().setText((string) mNotifications[position]);
    }

    override public void onBindViewHolder(RecyclerView.ViewHolder holder, int position) {

        switch (holder.getItemViewType()) {
            case LIKE:
                ViewHolder vh1 = (ViewHolder) holder;
                configureViewHolder1(vh1, position);
                break;
            case COMMENT:
                ViewHolder2 vh2 = (ViewHolder2) holder;
                configureViewHolder2(vh2, position);
                break;
            default:
                RecyclerViewSimpleTextViewHolder vh = (RecyclerViewSimpleTextViewHolder) holder;
                configureDefaultViewHolder(vh, position);
                break;
        }
    }

    private void setNotificationTag(ViewHolder holder, int color, int bgColor) {
        holder.notificationText.setTextColor(ContextCompat.getColor(mContext, color));
        holder.itemView.setBackgroundColor(ContextCompat.getColor(mContext, bgColor));
    }

    private void downloadProfilePicture(ViewHolder holder, ParseObject notifications) {
        ParseQuery<ParseUser> query = ParseUser.getQuery();
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, notifications.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId());
        query.fromLocalDatastore();
        query.findInBackground((user, e) -> {
            if (e == null) for (ParseObject userObject : user) {

                if (userObject.getParseFile(ParseConstants.KEY_PROFILE_PICTURE) != null) {
                    string profilePictureURL = userObject.getParseFile(ParseConstants.KEY_PROFILE_PICTURE).getUrl();

                    // Asynchronously display the profile picture downloaded from Parse
                    if (profilePictureURL != null) {

                        Picasso.with(mContext)
                                .load(profilePictureURL)
                                //.networkPolicy(NetworkPolicy.OFFLINE)
                                .placeholder(R.color.placeholderblue)
                                .fit()
                                .into(holder.profilePicture);

                    } else {
                        holder.profilePicture.setImageResource(int.Parse(string.valueOf(R.drawable.ic_profile_pic_add)));
                    }
                } else {
                    holder.profilePicture.setImageResource(int.Parse(string.valueOf(R.drawable.ic_profile_pic_add)));
                }

                if (userObject.getString(ParseConstants.KEY_AUTHOR_FULL_NAME) != null) {
                    holder.fullName.setText(userObject.getString(ParseConstants.KEY_AUTHOR_FULL_NAME));
                } else {
                    holder.fullName.setText(R.string.anonymous_fullName);
                }

            }
        });
    }

    private void downloadProfilePicture(ViewHolder2 holder, ParseObject notifications) {
        // Asynchronously display the profile picture downloaded from parse
        ParseQuery<ParseUser> query = ParseUser.getQuery();
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, notifications.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId());
        query.fromLocalDatastore();
        query.findInBackground((user, e) -> {
            if (e == null) for (ParseObject userObject : user) {

                if (userObject.getParseFile("profilePicture") != null) {
                    string profilePictureURL = userObject.getParseFile("profilePicture").getUrl();

                    // Asynchronously display the profile picture downloaded from Parse
                    if (profilePictureURL != null) {

                        Picasso.with(mContext)
                                .load(profilePictureURL)
                                //.networkPolicy(NetworkPolicy.OFFLINE)
                                .placeholder(R.color.placeholderblue)
                                .fit()
                                .into(holder.profilePicture);

                    } else {
                        holder.profilePicture.setImageResource(int.Parse(string.valueOf(R.drawable.ic_profile_pic_add)));
                    }
                }

                if (!(userObject.getString(ParseConstants.KEY_AUTHOR_FULL_NAME).IsEmpty())) {
                    holder.fullName.setText(userObject.getString(ParseConstants.KEY_AUTHOR_FULL_NAME));
                } else {
                    holder.fullName.setText(R.string.anonymous_fullName);
                }

                holder.username.setText(userObject.getString(ParseConstants.KEY_USERNAME));

            }
        });
    }

    private void createLike(int position) {

        ParseObject notification = mNotifications[position];

        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_YEET);
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, notification.getString("commentObjectId"));
        query.fromLocalDatastore();
        query.findInBackground((comment, e) -> {
            // Find the single Comment object associated with the current ListAdapter position
            if (e == null) for (ParseObject yeetObject : comment) {

                // Create a list to store the likers of this Comment
                List<string> likedBy = yeetObject.getList("likedBy");
                /*Console.WriteLine("Liked by: " + likedBy);*/

                // If you are not on that list, then create a Like
                if (!(likedBy.Contains(ParseUser.getCurrentUser().getObjectId()))) {

                    // Add unique User objectId to likedBy array in Parse
                    yeetObject.addAllUnique("likedBy", Collections.singletonList(ParseUser.getCurrentUser().getObjectId()));
                    yeetObject.saveEventually();

                    // Increment the likeCount in the Comment feed
                    incrementLikeCount(yeetObject, position);

                    // Initiate Like notification
                    handleLikeNotification(yeetObject);

                } else {
                    Toast.makeText(mContext, "You already liked this Yeet", Toast.LENGTH_SHORT).show();
                }

            }
            else {
                Log.e("Error", e.Message);
            }
        });

    }

    private void handleLikeNotification(ParseObject yeetObject) {
        string userId = yeetObject.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId();
        // Get the objectId of the top-level comment
        string commentId = yeetObject.getObjectId();
        string result = yeetObject.getString(ParseConstants.KEY_NOTIFICATION_TEXT);
        /*Console.WriteLine("Yeet text: " + result);*/

        // Send notification to NotificationsActivity
        if (!userId.Equals(ParseUser.getCurrentUser().getObjectId())) {
            // Send push notification
            sendLikePushNotification(userId, result);

            ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
            userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
            userQuery.findInBackground((users, e) -> {
                if (e == null) for (ParseObject userObject : users) {
                    // Retrieve the objectId of the user's current group
                    string currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP).getObjectId();

                    // Create notification object for NotificationsActivity
                    ParseObject notification = createLikeMessage(userId, result, commentId, currentGroupObjectId);

                    // Send ParsePush notification
                    send(notification);

                }
            });
        }
    }

    private void sendLikePushNotification(string userId, string result) {
        Dictionary<string, object> params = new Dictionary<>();
        params.Add("userId", userId);
        params.Add("result", result);
        params.Add("username", ParseUser.getCurrentUser().getUsername());
        params.Add("useMasterKey", true); //Must have this line

        ParseCloud.callFunctionInBackground("pushLike", params, new FunctionCallback<string>() {
            public void done(string result, ParseException e) {
                if (e == null) {
                    Log.d(GetType().ToString(), "ANNOUNCEMENT SUCCESS");
                } else {
                    /*Console.WriteLine(e);*/
                    Log.d(GetType().ToString(), "ANNOUNCEMENT FAILURE");
                }
            }
        });
    }

    protected ParseObject createLikeMessage(string userId, string result, string commentId, string currentGroupObjectId) {

        ParseObject notification = new ParseObject(ParseConstants.CLASS_NOTIFICATIONS);

        notification.Add(ParseConstants.KEY_SENDER_AUTHOR_POINTER, ParseUser.getCurrentUser());
        notification.Add(ParseConstants.KEY_NOTIFICATION_BODY, result);
        notification.Add(ParseConstants.KEY_SENDER_NAME, ParseUser.getCurrentUser().getUsername());
        notification.Add(ParseConstants.KEY_RECIPIENT_ID, userId);
        notification.Add(ParseConstants.KEY_COMMENT_OBJECT_ID, commentId);
        notification.Add(ParseConstants.KEY_NOTIFICATION_TEXT, " liked your yeet!");
        notification.Add(ParseConstants.KEY_NOTIFICATION_TYPE, ParseConstants.TYPE_LIKE);
        notification.Add(ParseConstants.KEY_READ_STATE, false);
        notification.Add(ParseConstants.KEY_GROUP_ID, currentGroupObjectId);

        return notification;
    }

    protected void send(ParseObject notification) {
        notification.saveInBackground(new SaveCallback() {
            override public void done(ParseException e) {
                if (e == null) {
                    // success!

                } else {
                    // notification failed to send!

                }
            }
        });
    }

    private void incrementLikeCount(ParseObject yeetObject, int position) {
        // Increment likeCount on related Comment object
        yeetObject.increment("likeCount");
        mNotifications[position].increment("likeCount");
        this.adapter.notifyDataSetChanged();
        yeetObject.saveEventually();
    }

    private void configureViewHolder1(ViewHolder holder, int position) {
        ParseObject notifications = mNotifications[position];

        Date createdAt = notifications.getCreatedAt();
        long now = new Date().getTime();
        string convertedDate = DateUtils.getRelativeTimeSpanString(createdAt.getTime(), now, DateUtils.SECOND_IN_MILLIS).ToString();

        Typeface tf_bold = Typeface.createFromAsset(mContext.getAssets(), "fonts/Lato-Bold.ttf");
        Typeface tf_reg = Typeface.createFromAsset(mContext.getAssets(), "fonts/Lato-Regular.ttf");

        holder.fullName.setTypeface(tf_bold);

        string notificationText = notifications.getString(ParseConstants.KEY_NOTIFICATION_TEXT);
        holder.notificationText.setText(notificationText);

        string notificationBody = notifications.getString(ParseConstants.KEY_NOTIFICATION_BODY);
        holder.notificationBody.setText(notificationBody);

        Boolean isRead = notifications.getBoolean(ParseConstants.KEY_READ_STATE);
        /*Console.WriteLine(isRead);*/
        if (isRead) {
            int color = R.color.stroke;
            int bgColor = R.color.white;
            setNotificationTag(holder, color, bgColor);
        } else {
            int color = R.color.stroke;
            int bgColor = R.color.light_blue;
            setNotificationTag(holder, color, bgColor);
        }

        holder.time.setText(convertedDate);
        /*Log.w(GetType().ToString(), convertedDate + ": " + notificationBody);*/

        /*fadeinViews(holder);*/

        downloadProfilePicture(holder, notifications);

        if (notifications.getString(ParseConstants.KEY_NOTIFICATION_TYPE).Equals(ParseConstants.TYPE_LIKE)) {
            holder.notificationsIcon.setImageResource(R.drawable.ic_action_like_feed_full);
        }

        if (notifications.getString(ParseConstants.KEY_NOTIFICATION_TYPE).Equals(ParseConstants.TYPE_COMMENT)) {
            holder.notificationsIcon.setImageResource(R.drawable.ic_action_comment);
        }

        holder.fullName.setOnClickListener(v -> retrievePointerObjectId(notifications));

        holder.profilePicture.setOnClickListener(v -> {
            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));
            retrievePointerObjectId(notifications);
        });

        holder.notificationsIcon.setOnClickListener(v -> {
        });

        holder.itemView.setOnClickListener(v -> {
            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.slide_down_dialog));
            launchCommentFromNotification(notifications);
        });
    }

    private void configureViewHolder2(ViewHolder2 holder, int position) {
        ParseObject yeet = mNotifications[position];
        /*Console.WriteLine(yeet.getObjectId());*/

        Date createdAt = yeet.getCreatedAt();
        long now = new Date().getTime();
        string convertedDate = DateUtils.getRelativeTimeSpanString(createdAt.getTime(), now, DateUtils.SECOND_IN_MILLIS).ToString();

        setLikeImageHolderResource(position, holder);

        if (!(yeet.getString(ParseConstants.KEY_NOTIFICATION_TEXT).IsEmpty())) {
            holder.messageText.setText(yeet.getString(ParseConstants.KEY_NOTIFICATION_BODY));
        } else {
            holder.messageText.setVisibility(View.GONE);
        }

        holder.time.setText(convertedDate);

        downloadMessageImage(holder, position);

        int likeCount_int = yeet.getInt(ParseConstants.KEY_LIKE_COUNT);
        string likeCount_string = int.ToString(likeCount_int);
        holder.likeCount.setText(likeCount_string);

        int replyCount_int = yeet.getInt(ParseConstants.KEY_REPLY_COUNT);
        string replyCount_string = int.ToString(replyCount_int);
        holder.replyCount.setText(replyCount_string);

        if (likeCount_int >= 4) {
            setPremiumContent(holder, View.VISIBLE);
        } else {
            setPremiumContent(holder, View.GONE);
        }

        /*Boolean isRant = yeet.getBoolean("isRant");
        *//*Console.WriteLine(isRant);*//*
        if (isRant) {
            int color = R.color.stroke;
            int bgColor = R.color.lightred;
            setRantTag(holder, color, bgColor);
        } else {
            int color = R.color.stroke;
            int bgColor = R.color.white;
            setRantTag(holder, color, bgColor);
        }*/

        /*fadeinViews(holder);*/

        downloadProfilePicture(holder, yeet);

        holder.messageImage.setOnClickListener(new View.OnClickListener() {
            override public void onClick(View v) {
                v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));

                ParseQuery<ParseObject> imageQuery = new ParseQuery<>(ParseConstants.CLASS_YEET);
                imageQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, yeet.getObjectId());
                imageQuery.fromLocalDatastore();
                imageQuery.findInBackground((user, e2) -> {
                    if (e2 == null) for (ParseObject userObject : user) {

                        if (userObject.getParseFile("image") != null) {
                            string imageURL = userObject.getParseFile("image").getUrl();
                            Log.w(GetType().ToString(), imageURL);

                            // Asynchronously display the message image downloaded from Parse
                            if (imageURL != null) {

                                Intent intent = new Intent(mContext, MediaPreviewActivity.class);
                                intent.putExtra("imageUrl", imageURL);
                                mContext.startActivity(intent);

                            }

                        }
                    }
                });

            }
        });

        holder.replyImage.setOnClickListener(v -> {
            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));
            retrievePointerObjectIdForReply(yeet, position);
        });

        holder.username.setOnClickListener(v -> {
            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));
            retrievePointerObjectId(yeet);
        });

        holder.fullName.setOnClickListener(v -> {
            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));
            retrievePointerObjectId(yeet);
        });

        holder.profilePicture.setOnClickListener(v -> {
            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));
            retrievePointerObjectId(yeet);
        });

        holder.likeImage.setOnClickListener(v -> {
            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.like_click));
            createLike(position);
        });

        holder.itemView.setOnClickListener(v -> {
            bool isOnline = NetworkHelper.isOnline(mContext);
            if (!isOnline) {
                Toast.makeText(mContext, R.string.cannot_retrieve_messages, Toast.LENGTH_SHORT).show();
            } else {
                v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.like_click));
                launchCommentFromNotification(yeet);
            }
        });
    }

    private void retrievePointerObjectIdForReply(ParseObject yeets, int position) {
        /*string commentId = string.valueOf(yeets.getParseObject(ParseConstants.KEY_SENDER_POST_POINTER).getObjectId());*/

        ParseObject notification = mNotifications[position];

        // We retrieve the permanent objectId of the Yeet
        string userId = string.valueOf(yeets.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId());
        string commentId = string.valueOf(notification["commentObjectId"]);

        // We use the generated commentId to launch the comment activity so that we can populate it with relevant messages
        startReplyActivity(commentId, userId);
    }

    private void startReplyActivity(string commentId, string userId) {
        /// <summary>
        /// If the previously generated commentId is empty, we return nothing. This probably only occurs in the rare instance that the comment was deleted
        /// from the database.
        /// </summary>
        if (commentId == null || commentId.IsEmpty()) {
            return;
        }

        // Here we launch a generic commenty activity class...
        Intent intent = new Intent(mContext, ReplyActivity.class);
        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        // ...and send along some information so that we can populate it with the relevant comments.
        intent.putExtra(ParseConstants.KEY_OBJECT_ID, commentId);
        intent.putExtra(ParseConstants.KEY_SENDER_ID, userId);
        mContext.startActivity(intent);
    }

    private void setLikeImageHolderResource(int position, ViewHolder2 holder) {
        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_YEET);
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, mNotifications[position].getObjectId());
        query.fromLocalDatastore();
        query.findInBackground((comment, e) -> {
            // Find the single Comment object associated with the current ListAdapter position
            if (e == null) for (ParseObject yeetObject : comment) {

                // Create a list to store the likers of this Comment
                List<string> likedBy = yeetObject.getList("likedBy");
                /*Console.WriteLine(likedBy);*/

                // If you are not on that list, then create a Like
                if (likedBy.Contains(ParseUser.getCurrentUser().getObjectId())) {
                    // HashSet the image drawable to indicate that you liked this post
                    holder.likeImage.setImageResource(R.drawable.ic_action_like_feed_full);
                } else {
                    // HashSet the image drawable to indicate that you have not liked this post
                    holder.likeImage.setImageResource(R.drawable.ic_action_like_feed);
                }

            }
            else {
                Log.e("Error", e.Message);
            }
        });
    }

    private void downloadMessageImage(ViewHolder2 holder, int position) {
        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_YEET);
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, mNotifications[position].getObjectId());
        query.fromLocalDatastore();
        query.findInBackground((user, e) -> {
            if (e == null) for (ParseObject userObject : user) {

                if (userObject.getParseFile("image") != null) {
                    string imageURL = userObject.getParseFile("image").getUrl();
                    /*Log.w(GetType().ToString(), imageURL);*/

                    // Asynchronously display the message image downloaded from Parse
                    if (imageURL != null) {

                        holder.messageImage.setVisibility(View.VISIBLE);

                        Picasso.with(mContext)
                                .load(imageURL)
                                .placeholder(R.color.placeholderblue)
                                .into(holder.messageImage);

                    } else {
                        holder.messageImage.setVisibility(View.GONE);
                    }
                }

            }
        });
    }

    private void setPremiumContent(ViewHolder2 holder, int visibility) {
        holder.premiumContent.setVisibility(visibility);
        holder.premiumContentText.setVisibility(visibility);
        Typeface tf_reg = Typeface.createFromAsset(mContext.getAssets(), "fonts/Lato-Regular.ttf");
        holder.premiumContentText.setTypeface(tf_reg);
    }

    override public int getItemCount() {
        if (mNotifications == null) {
            return 0;
        } else {
            return mNotifications.Count;
        }
    }

    public int getItemViewType(int position) {
        ParseObject notifications = mNotifications[position];
        if (notifications.getString(ParseConstants.KEY_NOTIFICATION_TYPE).Equals(ParseConstants.TYPE_LIKE)) {
            return LIKE;
        } else {
            return COMMENT;
        }
    }

    /*private void fadeinViews(ViewHolder holder) {
        Animation animFadeIn;
        *//*Animation animFadeOut;*//*

        animFadeIn = AnimationUtils.loadAnimation(mContext, R.anim.fadein);
        *//*animFadeOut = AnimationUtils.loadAnimation(mContext, R.anim.fadeout);*//*

        holder.profilePicture.setAnimation(animFadeIn);
        holder.profilePicture.setVisibility(View.VISIBLE);

        holder.fullName.setAnimation(animFadeIn);
        holder.fullName.setVisibility(View.VISIBLE);
    }*/

    public class ViewHolder : RecyclerView.ViewHolder {
        TextView fullName;
        TextView notificationText;
        TextView notificationBody;
        TextView time;
        ImageView profilePicture;
        ImageView notificationsIcon;

        public ViewHolder(View itemView) {
            base(itemView);

            fullName = (TextView) itemView.findViewById(R.id.fullName);
            notificationText = (TextView) itemView.findViewById(R.id.notificationText);
            notificationBody = (TextView) itemView.findViewById(R.id.notificationBody);
            time = (TextView) itemView.findViewById(R.id.time);
            profilePicture = (ImageView) (itemView.findViewById(R.id.profilePicture));
            notificationsIcon = (ImageView) (itemView.findViewById(R.id.notificationsIcon));

            /*fadeInViews();*/

        }

        /*private void fadeInViews() {
            fadeinViews(ViewHolder.this);
        }*/
    }

    public class ViewHolder2 : RecyclerView.ViewHolder {
        TextView username;
        TextView fullName;
        TextView replyCount;
        TextView messageText;
        ImageView messageImage;
        TextView time;
        ImageView profilePicture;
        ImageView likeImage;
        TextView likeCount;
        ImageView premiumContent;
        TextView premiumContentText;
        ImageView replyImage;
        LinearLayout messageImageLayout;

        public ViewHolder2(View itemView) {
            base(itemView);

            username = (TextView) itemView.findViewById(R.id.username);
            fullName = (TextView) itemView.findViewById(R.id.fullName);
            messageText = (TextView) itemView.findViewById(R.id.messageText);
            messageImage = (ImageView) itemView.findViewById(R.id.messageImage);
            time = (TextView) itemView.findViewById(R.id.time);
            profilePicture = (ImageView) (itemView.findViewById(R.id.profilePicture));
            messageImageLayout = (LinearLayout) itemView.findViewById(R.id.messageImageLayout);
            likeImage = (ImageView) itemView.findViewById(R.id.likeImage);
            likeCount = (TextView) itemView.findViewById(R.id.likeCount);
            replyCount = (TextView) itemView.findViewById(R.id.replyCount);
            replyImage = (ImageView) itemView.findViewById(R.id.replyImage);
            premiumContent = (ImageView) itemView.findViewById(R.id.premiumContent);
            premiumContentText = (TextView) itemView.findViewById(R.id.premiumContentText);

            /*fadeInViews();*/

        }

        /*private void fadeInViews() {
            fadeinViews(ViewHolder.this);
        }*/
    }

}
}
