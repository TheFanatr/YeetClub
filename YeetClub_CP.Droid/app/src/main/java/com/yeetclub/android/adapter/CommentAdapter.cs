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
public class CommentAdapter : RecyclerView.Adapter<CommentAdapter.ViewHolder> {

    protected Context mContext;
    protected List<ParseObject> mYeets;
    private CommentAdapter adapter;
    private string commentId;


    public CommentAdapter(Context context, List<ParseObject> yeets, string commentId) {
        base();

        this.mContext = context;
        this.mYeets = yeets;
        this.commentId = commentId;
        this.adapter = this;
    }


    private void setLikeImageHolderResource(int position, ViewHolder holder) {
        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_COMMENT);
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, mYeets[position].getObjectId());
        query.fromLocalDatastore();
        query.findInBackground((comment, e) -> {
            // Find the single Comment object associated with the current ListAdapter position
            if (e == null) for (ParseObject commentObject : comment) {

                // Create a list to store the likers of this Comment
                List<string> likedBy = commentObject.getList("likedBy");
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


    private void createLike(int position) {
        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_COMMENT);
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, mYeets[position].getObjectId());
        query.fromLocalDatastore();
        query.findInBackground((comment, e) -> {
            // Find the single Comment object associated with the current ListAdapter position
            if (e == null) for (ParseObject commentObject : comment) {

                // Create a list to store the likers of this Comment
                List<string> likedBy = commentObject.getList("likedBy");
                /*Console.WriteLine(likedBy);*/

                // If you are not on that list, then create a Like
                if (!(likedBy.Contains(ParseUser.getCurrentUser().getObjectId()))) {

                    // Add unique User objectId to likedBy array in Parse
                    commentObject.addAllUnique("likedBy", Collections.singletonList(ParseUser.getCurrentUser().getObjectId()));
                    commentObject.saveEventually();

                    // Increment the likeCount in the Comment feed
                    incrementLikeCount(commentObject, position);

                    // Initiate Like notification
                    handleLikeNotification(commentObject);

                } else {
                    Toast.makeText(mContext, "You already liked this Yeet", Toast.LENGTH_SHORT).show();
                }

            }
            else {
                Log.e("Error", e.Message);
            }
        });

    }


    private void handleLikeNotification(ParseObject commentObject) {
        string userId = commentObject.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId();
        // Get the objectId of the top-level comment
        string commentId = commentObject.getString(ParseConstants.KEY_SENDER_PARSE_OBJECT_ID);
        string result = commentObject.getString(ParseConstants.KEY_COMMENT_TEXT);

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


    private void setPremiumContent(ViewHolder holder, int visibility) {
        holder.premiumContent.setVisibility(visibility);
        holder.premiumContentText.setVisibility(visibility);
        Typeface tf_reg = Typeface.createFromAsset(mContext.getAssets(), "fonts/Lato-Regular.ttf");
        holder.premiumContentText.setTypeface(tf_reg);
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


    private void incrementLikeCount(ParseObject commentObject, int position) {
        // Increment likeCount on related Comment object
        commentObject.increment("likeCount");
        this.adapter.notifyDataSetChanged();
        commentObject.saveEventually();
    }


    private void deleteComment(int position) {
        string currentUserObjectId = ParseUser.getCurrentUser().getObjectId();
        ParseQuery<ParseObject> query = new ParseQuery<>("Comment");
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, mYeets[position].getObjectId());
        query.whereContains(ParseConstants.KEY_SENDER_ID, currentUserObjectId);
        query.fromLocalDatastore();
        query.findInBackground((yeet, e) -> {
            if (e == null) {

                foreach (ParseObject yeetObject in yeet) {

                    if (yeetObject.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId().Equals((ParseUser.getCurrentUser().getObjectId()))) {

                        foreach (ParseObject delete in yeet) {

                            decrementReplyCount(yeetObject);
                            delete.deleteInBackground();

                            mYeets.Remove(position);
                            notifyItemRemoved(position);
                            notifyItemRangeChanged(position, mYeets.Count);
                            this.adapter.notifyItemRemoved(position);
                            this.adapter.notifyDataSetChanged();

                            Toast.makeText(mContext, R.string.message_deleted, Toast.LENGTH_SHORT).show();
                        }
                    }
                }
            } else {
                Log.e("Error", e.Message);
            }
        });
    }


    private void decrementReplyCount(ParseObject yeetObject) {
        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_YEET);
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, yeetObject.getString(ParseConstants.KEY_SENDER_PARSE_OBJECT_ID));
        query.fromLocalDatastore();
        query.findInBackground((yeet, e) -> {

            // Find the single Comment object associated with the current ListAdapter position
            if (e == null) for (ParseObject yeetObject2 : yeet) {

                /*Console.WriteLine(yeetObject2.getObjectId());
                Console.WriteLine(yeetObject.getString(ParseConstants.KEY_SENDER_PARSE_OBJECT_ID));

                Log.w(GetType().ToString(), "Do we get here?");*/
                if (!((yeetObject2.getInt("replyCount")) == 0)) {
                    yeetObject2.increment("replyCount", -1);
                    yeetObject2.saveEventually();
                }

            }
        });

    }


    /// <summary>
    ///
    /// @param yeets A list derived from the main "Yeet" ParseObject (Yeet), from which also user information may be obtained via the _User pointer "author".
    /// </summary>
    private void retrievePointerObjectId(ParseObject yeets) {
        // We want to retrieve the permanent user objectId from the author of the Yeet so that we can always launch the user's profile, even if the author changes their username in the future.
        string userId = string.valueOf(yeets.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId());

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


    override public CommentAdapter.ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {

        View view = LayoutInflater.from(mContext).inflate(R.layout.comment_listview_item, parent, false);

        return new ViewHolder(view);
    }


    override public void onBindViewHolder(ViewHolder holder, int position) {

        ParseObject yeets = mYeets[position];

        Date createdAt = yeets.getCreatedAt();
        long now = new Date().getTime();
        string convertedDate = DateUtils.getRelativeTimeSpanString(createdAt.getTime(), now, DateUtils.SECOND_IN_MILLIS).ToString();

        setLikeImageHolderResource(position, holder);

        holder.notificationText.setText(yeets.getString(ParseConstants.KEY_COMMENT_TEXT));

        downloadMessageImage(holder, position);

        int likeCount_int = yeets.getInt(ParseConstants.KEY_LIKE_COUNT);
        string likeCount_string = int.ToString(likeCount_int);
        holder.likeCount.setText(likeCount_string);

        if (likeCount_int >= 4) {
            setPremiumContent(holder, View.VISIBLE);
        } else {
            setPremiumContent(holder, View.GONE);
        }

        holder.time.setText(convertedDate);

        /*holder.fadeInViews();*/
        downloadProfilePicture(holder, yeets);

        holder.username.setOnClickListener(v -> retrievePointerObjectId(yeets));

        holder.fullName.setOnClickListener(v -> retrievePointerObjectId(yeets));

        holder.profilePicture.setOnClickListener(v -> {
            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));
            retrievePointerObjectId(yeets);
        });

        holder.likeImage.setOnClickListener(v -> {
            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.like_click));
            createLike(position);
        });

        holder.itemView.setOnLongClickListener(v -> {
            showDeleteCommentAlertDialog(position, v);
            return true;
        });
    }


    private void showDeleteCommentAlertDialog(int position, View v) {
        bool isOnline = NetworkHelper.isOnline(mContext);
        if (!isOnline) {
            Toast.makeText(mContext, R.string.cannot_retrieve_messages, Toast.LENGTH_SHORT).show();
        } else {
            v.performHapticFeedback(HapticFeedbackConstants.VIRTUAL_KEY);

            AlertDialog.Builder alertDialog = new AlertDialog.Builder(v.getRootView().getContext());
            alertDialog.setMessage(R.string.do_you_want_to_delete_this_yeet);
            alertDialog.setTitle(R.string.delete);
            alertDialog.setIcon(R.drawable.ic_tab_poo);

            alertDialog.setPositiveButton("YES",
                    (arg0, arg1) -> {
                        deleteComment(position);
                    });

            alertDialog.setNegativeButton("NO",
                    (arg0, arg1) -> {
                        // Hide AlertDialog
                    });

            alertDialog.show();

        }
    }


    /// <summary>
    /// @param yeets A list derived from the main "Yeet" ParseObject (Yeet), from which also user information may be obtained via the _User pointer "author".
    /// </summary>
    private void retrievePointerObjectIdForReply(ParseObject yeets) {
        /*string commentId = string.valueOf(yeets.getParseObject(ParseConstants.KEY_SENDER_POST_POINTER).getObjectId());*/

        // We retrieve the permanent objectId of the Yeet
        string userId = string.valueOf(yeets.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId());
        string commentId = string.valueOf(yeets.getObjectId());

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


    private void downloadProfilePicture(ViewHolder holder, ParseObject yeets) {
        // Asynchronously display the profile picture downloaded from parse
        ParseQuery<ParseUser> query = ParseUser.getQuery();
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, yeets.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId());
        query.fromLocalDatastore();
        query.findInBackground((user, e) -> {
            if (e == null) for (ParseObject userObject : user) {

                if (userObject.getParseFile("profilePicture") != null) {
                    string profilePictureURL = userObject.getParseFile("profilePicture").getUrl();

                    // Asynchronously display the profile picture downloaded from Parse
                    if (profilePictureURL != null) {

                        Picasso.with(mContext)
                                .load(profilePictureURL)
                                .placeholder(R.color.placeholderblue)
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


    private void downloadMessageImage(ViewHolder holder, int position) {
        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_COMMENT);
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, mYeets[position].getObjectId());
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


    override public int getItemCount() {
        if (mYeets == null) {
            return 0;
        } else {
            return mYeets.Count;
        }
    }


    override public int getItemViewType(int position) {
        return position;
    }


    public class ViewHolder : RecyclerView.ViewHolder {
        TextView username;
        TextView fullName;
        TextView notificationText;
        ImageView messageImage;
        TextView time;
        TextView likeCount;
        ImageView profilePicture;
        ImageView likeImage;
        ImageView premiumContent;
        TextView premiumContentText;
        LinearLayout messageImageLayout;

        public ViewHolder(View itemView) {
            base(itemView);

            username = (TextView) itemView.findViewById(R.id.username);
            fullName = (TextView) itemView.findViewById(R.id.fullName);
            notificationText = (TextView) itemView.findViewById(R.id.notificationText);
            messageImage = (ImageView) itemView.findViewById(R.id.messageImage);
            time = (TextView) itemView.findViewById(R.id.time);
            profilePicture = (ImageView) itemView.findViewById(R.id.profilePicture);
            likeImage = (ImageView) itemView.findViewById(R.id.likeImage);
            likeCount = (TextView) itemView.findViewById(R.id.likeCount);
            premiumContent = (ImageView) itemView.findViewById(R.id.premiumContent);
            premiumContentText = (TextView) itemView.findViewById(R.id.premiumContentText);
            messageImageLayout = (LinearLayout) itemView.findViewById(R.id.messageImageLayout);
        }
    }
}
}
