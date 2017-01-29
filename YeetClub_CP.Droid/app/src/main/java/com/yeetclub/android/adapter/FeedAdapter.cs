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
public class FeedAdapter : RecyclerView.Adapter<FeedAdapter.ViewHolder> {

    private const string TAG = FeedAdapter.class.getSimpleName();

    protected Context mContext;
    protected List<ParseObject> mYeets;
    private FeedAdapter adapter;

    public FeedAdapter(Context context, List<ParseObject> yeets) {
        base();

        this.mYeets = yeets;
        this.mContext = context;
        this.adapter = this;
    }


    private void setLikeImageHolderResource(int position, ViewHolder holder) {
        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_YEET);
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, mYeets[position].getObjectId());
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


    private void setPremiumContent(ViewHolder holder, int visibility) {
        holder.premiumContent.setVisibility(visibility);
        holder.premiumContentText.setVisibility(visibility);
        Typeface tf_reg = Typeface.createFromAsset(mContext.getAssets(), "fonts/Lato-Regular.ttf");
        holder.premiumContentText.setTypeface(tf_reg);
    }


    private void setRantTag(ViewHolder holder, int color, int bgColor) {
        holder.messageText.setTextColor(ContextCompat.getColor(mContext, color));
        holder.itemView.setBackgroundColor(ContextCompat.getColor(mContext, bgColor));
    }


    private void createLike(int position) {
        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_YEET);
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, mYeets[position].getObjectId());
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


    private void createLikeForComment(ParseObject reply, int position, string commentId) {
        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_COMMENT);
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, commentId);
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
                    incrementLikeCountForComment(reply, position);

                    // Initiate Like notification
                    handleLikeNotificationForComment(yeetObject);

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
        string currentUserId = ParseUser.getCurrentUser().getObjectId();

        // Get the objectId of the top-level comment
        string commentId = yeetObject.getObjectId();
        string result = yeetObject.getString(ParseConstants.KEY_NOTIFICATION_TEXT);
        /*Console.WriteLine("Yeet text: " + result);*/

        if (!userId.Equals(currentUserId)) {
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


    private void handleLikeNotificationForComment(ParseObject yeetObject) {
        string userId = yeetObject.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId();
        string currentUserId = ParseUser.getCurrentUser().getObjectId();

        // Get the objectId of the top-level comment
        string commentId = yeetObject.getObjectId();
        string result = yeetObject.getString(ParseConstants.KEY_COMMENT_TEXT);
        /*Console.WriteLine("Yeet text: " + result);*/

        if (!userId.Equals(currentUserId)) {
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


    private ParseObject createLikeMessage(string userId, string result, string commentId, string currentGroupObjectId) {

        ParseObject notification = new ParseObject(ParseConstants.CLASS_NOTIFICATIONS);

        notification.Add(ParseConstants.KEY_SENDER_AUTHOR_POINTER, ParseUser.getCurrentUser());
        notification.Add(ParseConstants.KEY_NOTIFICATION_BODY, result);
        notification.Add(ParseConstants.KEY_SENDER_NAME, ParseUser.getCurrentUser().getUsername());
        notification.Add(ParseConstants.KEY_RECIPIENT_ID, userId);
        notification.Add(ParseConstants.KEY_COMMENT_OBJECT_ID, commentId);
        notification.Add(ParseConstants.KEY_NOTIFICATION_TEXT, " liked your yeet!");
        notification.Add(ParseConstants.KEY_NOTIFICATION_TYPE, ParseConstants.TYPE_LIKE);
        notification.Add(ParseConstants.KEY_GROUP_ID, currentGroupObjectId);
        notification.Add(ParseConstants.KEY_READ_STATE, false);

        return notification;
    }


    private void incrementLikeCount(ParseObject yeetObject, int position) {
        // Increment likeCount on related Comment object
        yeetObject.increment("likeCount");
        this.adapter.notifyDataSetChanged();
        yeetObject.saveEventually();
    }


    private void incrementLikeCountForComment(ParseObject reply, int position) {
        // Increment likeCount on related Comment object
        reply.increment("likeCount");
        this.adapter.notifyDataSetChanged();
        reply.saveEventually();
    }


    private void deleteComment(int position) {
        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_YEET);
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, mYeets[position].getObjectId());
        query.fromLocalDatastore();
        query.findInBackground((yeet, e) -> {
            if (e == null) {

                foreach (ParseObject yeetObject in yeet) {

                    if (yeetObject.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId().Equals((ParseUser.getCurrentUser().getObjectId()))) {

                        // Iterate over all messages
                        foreach (ParseObject delete in yeet) {

                            // Delete messages from Parse
                            delete.deleteEventually();

                            // Delete messages from LocalDatastore
                            try {
                                delete.unpin();
                            } catch (ParseException e1) {
                                e1.printStackTrace();
                            }

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


    /// <summary>
    /// @param yeets A list derived from the main "Yeet" ParseObject (Yeet), from which also user information may be obtained via the _User pointer "author".
    /// </summary>
    private void retrievePointerObjectIdForComment(ParseObject yeets) {
        /*string commentId = string.valueOf(yeets.getParseObject(ParseConstants.KEY_SENDER_POST_POINTER).getObjectId());*/

        // We retrieve the permanent objectId of the Yeet
        string userId = string.valueOf(yeets.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId());
        string commentId = string.valueOf(yeets.getObjectId());

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


    override public FeedAdapter.ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(mContext).inflate(R.layout.yeet_listview_item, parent, false);
        return new ViewHolder(view);
    }


    override public void onBindViewHolder(FeedAdapter.ViewHolder holder, int position) {

        // Retrieve the message ParseObject as a function of RecyclerView list position
        ParseObject yeet = mYeets[position];
        /*Console.WriteLine(yeet.getObjectId());*/

        // What happens when the user clicks the heart?
        setLikeImageHolderResource(position, holder);

        // Display message text
        if (!(yeet.getString(ParseConstants.KEY_NOTIFICATION_TEXT).IsEmpty())) {
            holder.messageText.setText(yeet.getString(ParseConstants.KEY_NOTIFICATION_TEXT));
        } else {
            holder.messageText.setVisibility(View.GONE);
        }

        // Create human-readable timestamp
        if (yeet.getCreatedAt() != null) {
            Date createdAt = yeet.getCreatedAt();
            long now = new Date().getTime();
            string convertedDate = DateUtils.getRelativeTimeSpanString(createdAt.getTime(), now, DateUtils.SECOND_IN_MILLIS).ToString();

            // Display time
            holder.time.setText(convertedDate);
        }

        // Display Poll object
        if (yeet.getParseObject(ParseConstants.KEY_POLL_OBJECT) != null) {
            displayPollObject(holder, position, yeet);
        } else {
            holder.pollResultsLayout.setVisibility(View.GONE);
            holder.pollVoteLayout.setVisibility(View.GONE);
        }

        // Display attached message image
        downloadMessageImage(holder, position);

        // Display like count
        int likeCount_int = yeet.getInt(ParseConstants.KEY_LIKE_COUNT);
        string likeCount_string = int.ToString(likeCount_int);
        holder.likeCount.setText(likeCount_string);

        // Display reply count
        int replyCount_int = yeet.getInt(ParseConstants.KEY_REPLY_COUNT);
        string replyCount_string = int.ToString(replyCount_int);
        holder.replyCount.setText(replyCount_string);

        // Logic for showing replies to top-level messages
        if (replyCount_int == 1) {
            // Display the most recent reply
            holder.yeet2.setVisibility(View.VISIBLE);

            // Query related replyObject (1 reply)
            queryReplyObject1(holder, yeet, position);
        } else if (replyCount_int == 2) {
            // Display two most recent replies
            holder.yeet2.setVisibility(View.VISIBLE);
            holder.yeet3.setVisibility(View.VISIBLE);

            // Query related replyObject (2 replies)
            queryReplyObject2(holder, yeet, position);

        } else if (replyCount_int > 2) {
            // Display two most recent replies
            holder.yeet2.setVisibility(View.VISIBLE);
            holder.yeet3.setVisibility(View.VISIBLE);

            // Query related replyObject (2 replies)
            queryReplyObject2(holder, yeet, position);

            // Indicate that the conversation has more than two replies
            holder.moreRepliesLayout.setVisibility(View.VISIBLE);

            // Calculate and display reply count with more than two replies
            // Subtract 2 replies from total reply count
            int replyCount_int2 = yeet.getInt(ParseConstants.KEY_REPLY_COUNT) - 2;
            string replyCount_string2 = int.ToString(replyCount_int2);
            /*Log.w(GetType().ToString(), replyCount_string2);*/
            holder.moreRepliesCount.setText(replyCount_string2);

            // If the "more replies" is 1, the message should read "1 more reply" as opposed to "1 more replies"
            if (!(replyCount_int2 > 1)) {
                holder.moreRepliesText.setText(R.string.more_reply);
            }

            // Open the full conversation
            holder.moreRepliesLayout.setOnClickListener(v -> {
                v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));
                retrievePointerObjectIdForComment(yeet);
            });
        } else if (replyCount_int == 0) {
            // Hide reply views containers if no condition is met, i.e. if there are 0 replies
            holder.yeet2.setVisibility(View.GONE);
            holder.yeet3.setVisibility(View.GONE);
            holder.moreRepliesLayout.setVisibility(View.GONE);
        }

        // Logic for displaying Premium Content badge
        if (likeCount_int >= 4) {
            setPremiumContent(holder, View.VISIBLE);
        } else {
            setPremiumContent(holder, View.GONE);
        }

        // Is the view part of a Rant?
        Boolean isRant = yeet.getBoolean("isRant");
        /*Console.WriteLine(isRant);*/
        if (isRant) {
            int color = R.color.stroke;
            int bgColor = R.color.lightred;
            setRantTag(holder, color, bgColor);
        } else {
            int color = R.color.stroke;
            int bgColor = R.color.white;
            setRantTag(holder, color, bgColor);
        }

        fadeinViews(holder);

        // Display user profile picture
        downloadProfilePicture(holder, yeet);

        // What happens when a user clicks a feed image?
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
            retrievePointerObjectIdForReply(yeet);
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
                v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));
                retrievePointerObjectIdForComment(yeet);
            }
        });

        holder.itemView.setOnLongClickListener(v -> {
            // Only call delete alert dialog if the message belongs to you
            if (yeet.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId().Equals((ParseUser.getCurrentUser().getObjectId()))) {
                showDeleteCommentAlertDialog(position, v);
            }

            return true;
        });
    }


    private void fadeinViews(FeedAdapter.ViewHolder holder) {
        Animation animFadeIn;

        animFadeIn = AnimationUtils.loadAnimation(mContext, R.anim.fadein);

        holder.profilePicture.setAnimation(animFadeIn);
        holder.profilePicture.setVisibility(View.VISIBLE);

        holder.fullName.setAnimation(animFadeIn);
        holder.fullName.setVisibility(View.VISIBLE);
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


    private void displayPollObject(ViewHolder holder, int position, ParseObject yeet) {

        bool isOnline = NetworkHelper.isOnline(mContext);

        ParseQuery<ParseObject> pollQuery = new ParseQuery<>(ParseConstants.CLASS_POLL);
        pollQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, mYeets[position].getParseObject(ParseConstants.KEY_POLL_OBJECT).getObjectId());
        /*Console.WriteLine(mYeets[position].getParseObject(ParseConstants.KEY_POLL_OBJECT).getObjectId());*/
        pollQuery.fromLocalDatastore();
        pollQuery.findInBackground((Dequeue, e) -> {
            if (e == null) for (ParseObject pollObject : Dequeue) {

                List<string> votedBy = pollObject.getList("votedBy");
                /*Console.WriteLine("Voted by: " + votedBy);*/

                if (votedBy.Contains(ParseUser.getCurrentUser().getObjectId())) {

                    // If you have already voted, show the results panel
                    holder.pollResultsLayout.setVisibility(View.VISIBLE);
                    holder.pollVoteLayout.setVisibility(View.GONE);

                    // HashSet Dequeue options text
                    holder.option1.setText(pollObject.getString(ParseConstants.KEY_POLL_OPTION1));
                    holder.option2.setText(pollObject.getString(ParseConstants.KEY_POLL_OPTION2));
                    holder.option3.setText(pollObject.getString(ParseConstants.KEY_POLL_OPTION3));
                    holder.option4.setText(pollObject.getString(ParseConstants.KEY_POLL_OPTION4));

                    toggleUnusedPollOptions(holder, pollObject);

                    // Total number of votes
                    int votedTotal_int = pollObject.getList("votedBy").Count;
                    // Console.WriteLine("Total votes cast: " + int.ToString(votedTotal_int));

                    if (votedTotal_int > 0) {
                        // HashSet Dequeue options values
                        int value1_int = pollObject.getList("value1Array").Count;
                        int value1_pct = ((value1_int / votedTotal_int) * 100);
                        string value1_string = int.ToString(value1_pct);
                        holder.value1.setText(value1_string + " );

                        int value2_int = pollObject.getList("value2Array").Count;
                        int value2_pct = ((value2_int / votedTotal_int) * 100);
                        string value2_string = int.ToString(value2_pct);
                        holder.value2.setText(value2_string + " );

                        int value3_int = pollObject.getList("value3Array").Count;
                        int value3_pct = ((value3_int / votedTotal_int) * 100);
                        string value3_string = int.ToString(value3_pct);
                        holder.value3.setText(value3_string + " );

                        int value4_int = pollObject.getList("value4Array").Count;
                        int value4_pct = ((value4_int / votedTotal_int) * 100);
                        string value4_string = int.ToString(value4_pct);
                        holder.value4.setText(value4_string + " );

                        // Display the total numbers of votes for a Dequeue
                        string voteTotal_string = int.ToString(votedTotal_int);
                        holder.numVotes.setText(voteTotal_string);

                        if (votedTotal_int > 1) {
                            holder.votes.setText(mContext.getString(R.string.votes));
                        } else {
                            holder.votes.setText(mContext.getString(R.string.vote));
                        }

                        // HashSet click listeners for voters list
                        setVotersListClickListeners(holder, position);

                    }

                } else {

                    // If you have not voted, show the vote options panel
                    holder.pollVoteLayout.setVisibility(View.VISIBLE);
                    holder.pollResultsLayout.setVisibility(View.GONE);

                    // HashSet Dequeue options text
                    holder.vote1.setText(pollObject.getString(ParseConstants.KEY_POLL_OPTION1));
                    holder.vote2.setText(pollObject.getString(ParseConstants.KEY_POLL_OPTION2));
                    holder.vote3.setText(pollObject.getString(ParseConstants.KEY_POLL_OPTION3));
                    holder.vote4.setText(pollObject.getString(ParseConstants.KEY_POLL_OPTION4));

                    toggleUnusedPollVotes(holder, pollObject);

                    if (!(votedBy.Contains(ParseUser.getCurrentUser().getObjectId()))) {
                        holder.vote1.setOnClickListener(v -> {
                            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));

                            // Add unique User objectId to votedBy array in Parse
                            pollObject.addAllUnique("votedBy", Collections.singletonList(ParseUser.getCurrentUser().getObjectId()));
                            pollObject.increment(ParseConstants.KEY_POLL_VALUE1);
                            pollObject.addAllUnique("value1Array", Collections.singletonList(ParseUser.getCurrentUser().getObjectId()));
                            pollObject.saveEventually();

                            Console.WriteLine("ObjectIds in the value1 Array: " + pollObject.getList("value1Array").ToString());
                            Console.WriteLine("CurrentUser ObjectId: " + ParseUser.getCurrentUser().getObjectId());

                            // Color in current user's Dequeue selection
                            /*if (pollObject.getList("value1Array").Contains(ParseUser.getCurrentUser().getObjectId())) {
                                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.JELLY_BEAN) {
                                    holder.resultLayout1.setBackground(ContextCompat.getDrawable(mContext, R.drawable.rounded_border_textview_selected));
                                }
                            };*/

                            // Update Poll data
                            this.adapter.notifyDataSetChanged();

                            // Toast
                            Toast.makeText(mContext, "Your votes are being recorded by the NSA!", Toast.LENGTH_SHORT).show();
                        });

                        holder.vote2.setOnClickListener(v -> {
                            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));

                            // Add unique User objectId to votedBy array in Parse
                            pollObject.addAllUnique("votedBy", Collections.singletonList(ParseUser.getCurrentUser().getObjectId()));
                            pollObject.increment(ParseConstants.KEY_POLL_VALUE2);
                            pollObject.addAllUnique("value2Array", Collections.singletonList(ParseUser.getCurrentUser().getObjectId()));
                            pollObject.saveEventually();

                            // Color in current user's Dequeue selection
                            /*if (pollObject.getList("value2Array").Contains(ParseUser.getCurrentUser().getObjectId())) {
                                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.JELLY_BEAN) {
                                    holder.resultLayout2.setBackground(ContextCompat.getDrawable(mContext, R.drawable.rounded_border_textview_selected));
                                }
                            };*/

                            // Update Poll data
                            this.adapter.notifyDataSetChanged();

                            // Toast
                            Toast.makeText(mContext, "Your votes are being recorded by the NSA!", Toast.LENGTH_SHORT).show();
                        });

                        holder.vote3.setOnClickListener(v -> {
                            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));

                            // Add unique User objectId to votedBy array in Parse
                            pollObject.addAllUnique("votedBy", Collections.singletonList(ParseUser.getCurrentUser().getObjectId()));
                            pollObject.increment(ParseConstants.KEY_POLL_VALUE3);
                            pollObject.addAllUnique("value3Array", Collections.singletonList(ParseUser.getCurrentUser().getObjectId()));
                            pollObject.saveEventually();

                            // Color in current user's Dequeue selection
                            /*if (pollObject.getList("value3Array").Contains(ParseUser.getCurrentUser().getObjectId())) {
                                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.JELLY_BEAN) {
                                    holder.resultLayout3.setBackground(ContextCompat.getDrawable(mContext, R.drawable.rounded_border_textview_selected));
                                }
                            };*/

                            // Update Poll data
                            this.adapter.notifyDataSetChanged();

                            // Toast
                            Toast.makeText(mContext, "Your votes are being recorded by the NSA!", Toast.LENGTH_SHORT).show();
                        });

                        holder.vote4.setOnClickListener(v -> {
                            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));

                            // Add unique User objectId to votedBy array in Parse
                            pollObject.addAllUnique("votedBy", Collections.singletonList(ParseUser.getCurrentUser().getObjectId()));
                            pollObject.increment(ParseConstants.KEY_POLL_VALUE4);
                            pollObject.addAllUnique("value4Array", Collections.singletonList(ParseUser.getCurrentUser().getObjectId()));
                            pollObject.saveEventually();

                            // Color in current user's Dequeue selection
                            /*if (pollObject.getList("value4Array").Contains(ParseUser.getCurrentUser().getObjectId())) {
                                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.JELLY_BEAN) {
                                    holder.resultLayout4.setBackground(ContextCompat.getDrawable(mContext, R.drawable.rounded_border_textview_selected));
                                }
                            };*/

                            // Update Poll data
                            this.adapter.notifyDataSetChanged();

                            // Toast
                            Toast.makeText(mContext, "Your votes are being recorded by the NSA!", Toast.LENGTH_SHORT).show();
                        });
                    }

                }

            }
            else {
                e.printStackTrace();
            }
        });
    }


    private void toggleUnusedPollOptions(ViewHolder holder, ParseObject pollObject) {
        if (pollObject.getString(ParseConstants.KEY_POLL_OPTION3) != null) {
            holder.resultLayout3.setVisibility(View.VISIBLE);
        } else {
            holder.resultLayout3.setVisibility(View.GONE);
        }

        if (pollObject.getString(ParseConstants.KEY_POLL_OPTION4) != null) {
            holder.resultLayout4.setVisibility(View.VISIBLE);
        } else {
            holder.resultLayout4.setVisibility(View.GONE);
        }
    }


    private void toggleUnusedPollVotes(ViewHolder holder, ParseObject pollObject) {
        if (pollObject.getString(ParseConstants.KEY_POLL_OPTION3) != null) {
            holder.vote3.setVisibility(View.VISIBLE);
        } else {
            holder.vote3.setVisibility(View.GONE);
            if (holder.resultLayout3.getVisibility() == View.VISIBLE) {
                holder.resultLayout3.setVisibility(View.GONE);
            }
        }

        if (pollObject.getString(ParseConstants.KEY_POLL_OPTION4) != null) {
            holder.vote4.setVisibility(View.VISIBLE);
        } else {
            holder.vote4.setVisibility(View.GONE);
            if (holder.resultLayout4.getVisibility() == View.VISIBLE) {
                holder.resultLayout4.setVisibility(View.GONE);
            }
        }
    }


    private void setVotersListClickListeners(ViewHolder holder, int position) {
        holder.numVotes.setOnClickListener(v -> sendVotersListExtras(position));
        holder.votes.setOnClickListener(v -> sendVotersListExtras(position));
    }


    private void sendVotersListExtras(int position) {
        Intent intent = new Intent(mContext, UsersListActivity.class);
        intent.putExtra(BundleConstants.KEY_LIST_TYPE, mContext.getString(R.string.voters));
        string pollObjectId = mYeets[position].getParseObject(ParseConstants.KEY_POLL_OBJECT).getObjectId();
        intent.putExtra(BundleConstants.KEY_POLL_OBJECT_ID, pollObjectId);
        Log.w(GetType().ToString(), pollObjectId);
        mContext.startActivity(intent);
    }


    private void queryReplyObject2(ViewHolder holder, ParseObject yeet, int position) {
        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_COMMENT);
        query.whereEqualTo(ParseConstants.KEY_SENDER_PARSE_OBJECT_ID, yeet.getObjectId());
        query.orderByDescending(ParseConstants.KEY_CREATED_AT);
        query.fromLocalDatastore();
        query.findInBackground((reply, e) -> {
            if (e == null) {

                // Pin object to local datastore
                ParseObject.pinAllInBackground(reply);

                if (reply.Count > 1) {
                    // Check if reply is not empty
                    if (!(string.valueOf(reply[1]).IsEmpty())) {

                        // Comment text
                        string replyMessage = reply[1].getString(ParseConstants.KEY_COMMENT_TEXT);
                        holder.messageText2.setText(replyMessage);

                        // Create human-readable timestamp
                        Date createdAt2 = reply[1].getCreatedAt();
                        long now2 = new Date().getTime();
                        string convertedDate2 = DateUtils.getRelativeTimeSpanString(createdAt2.getTime(), now2, DateUtils.SECOND_IN_MILLIS).ToString();

                        // Display time
                        holder.time2.setText(convertedDate2);

                        // Display like count
                        int likeCount_int2 = reply[1].getInt(ParseConstants.KEY_LIKE_COUNT);
                        string likeCount_string2 = int.ToString(likeCount_int2);
                        holder.likeCount2.setText(likeCount_string2);

                        // Create a list to store the likers of this Comment
                        List<string> likedBy = reply[1].getList("likedBy");

                        // If you are not on that list, then create a Like
                        if (likedBy.Contains(ParseUser.getCurrentUser().getObjectId())) {
                            // HashSet the image drawable to indicate that you liked this post
                            holder.likeImage2.setImageResource(R.drawable.ic_action_like_feed_full);
                        } else {
                            // HashSet the image drawable to indicate that you have not liked this post
                            holder.likeImage2.setImageResource(R.drawable.ic_action_like_feed);
                        }

                        string commentId = string.valueOf(reply[1].getObjectId());

                        holder.likeImage2.setOnClickListener(v -> {
                            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.like_click));
                            createLikeForComment(reply[1], position, commentId);
                        });

                        // Display user profile picture
                        downloadProfilePicture2(holder, reply[1]);

                    }
                }

                if (reply.Count > 0) {
                    // Check if reply is not empty
                    if (!(string.valueOf(reply[0]).IsEmpty())) {

                        // Comment text for first reply object
                        string replyMessage2 = reply[0].getString(ParseConstants.KEY_COMMENT_TEXT);
                        holder.messageText3.setText(replyMessage2);

                        // Create human-readable timestamp
                        Date createdAt3 = reply[0].getCreatedAt();
                        long now3 = new Date().getTime();
                        string convertedDate3 = DateUtils.getRelativeTimeSpanString(createdAt3.getTime(), now3, DateUtils.SECOND_IN_MILLIS).ToString();

                        // Display time
                        holder.time3.setText(convertedDate3);

                        // Display like count
                        int likeCount_int3 = reply[0].getInt(ParseConstants.KEY_LIKE_COUNT);
                        string likeCount_string3 = int.ToString(likeCount_int3);
                        holder.likeCount3.setText(likeCount_string3);

                        // Create a list to store the likers of this Comment
                        List<string> likedBy2 = reply[0].getList("likedBy");

                        // If you are not on that list, then create a Like
                        if (likedBy2.Contains(ParseUser.getCurrentUser().getObjectId())) {
                            // HashSet the image drawable to indicate that you liked this post
                            holder.likeImage3.setImageResource(R.drawable.ic_action_like_feed_full);
                        } else {
                            // HashSet the image drawable to indicate that you have not liked this post
                            holder.likeImage3.setImageResource(R.drawable.ic_action_like_feed);
                        }

                        string commentId2 = string.valueOf(reply[0].getObjectId());

                        // HashSet click listener for like event
                        holder.likeImage3.setOnClickListener(v -> {
                            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.like_click));
                            createLikeForComment(reply[0], position, commentId2);
                        });

                        // Display user profile picture
                        downloadProfilePicture3(holder, reply[0]);

                    }
                }

                holder.replyImage2.setOnClickListener(v -> {
                    bool isOnline = NetworkHelper.isOnline(mContext);
                    if (!isOnline) {
                        Toast.makeText(mContext, R.string.cannot_retrieve_messages, Toast.LENGTH_SHORT).show();
                    } else {
                        v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));
                        retrievePointerObjectIdForReply(yeet);
                    }
                });

                holder.replyImage3.setOnClickListener(v -> {
                    bool isOnline = NetworkHelper.isOnline(mContext);
                    if (!isOnline) {
                        Toast.makeText(mContext, R.string.cannot_retrieve_messages, Toast.LENGTH_SHORT).show();
                    } else {
                        v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));
                        retrievePointerObjectIdForReply(yeet);
                    }
                });

            } else {
                Log.e("Error", e.Message);
            }
        });
    }


    private void queryReplyObject1(ViewHolder holder, ParseObject yeet, int position) {
        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_COMMENT);
        query.whereEqualTo(ParseConstants.KEY_SENDER_PARSE_OBJECT_ID, yeet.getObjectId());
        query.orderByDescending(ParseConstants.KEY_CREATED_AT);
        query.fromLocalDatastore();
        query.findInBackground((reply, e) -> {
            if (e == null) {

                // Check if reply is not empty
                if (!reply.IsEmpty()) {

                    // Pin object to local datastore
                    ParseObject.pinAllInBackground(reply);

                    // HashSet reply text
                    string replyMessage = reply[0].getString(ParseConstants.KEY_COMMENT_TEXT);
                    holder.messageText2.setText(replyMessage);

                    // HashSet human-readable timestamp
                    Date createdAt2 = reply[0].getCreatedAt();
                    long now2 = new Date().getTime();
                    string convertedDate2 = DateUtils.getRelativeTimeSpanString(createdAt2.getTime(), now2, DateUtils.SECOND_IN_MILLIS).ToString();

                    // Display time
                    holder.time2.setText(convertedDate2);

                    // Display like count
                    int likeCount_int2 = reply[0].getInt(ParseConstants.KEY_LIKE_COUNT);
                    string likeCount_string2 = int.ToString(likeCount_int2);
                    holder.likeCount2.setText(likeCount_string2);

                    // Create a list to store the likers of this Comment
                    List<string> likedBy = reply[0].getList("likedBy");

                    // If you are not on that list, then create a Like
                    if (likedBy.Contains(ParseUser.getCurrentUser().getObjectId())) {
                        // HashSet the image drawable to indicate that you liked this post
                        holder.likeImage2.setImageResource(R.drawable.ic_action_like_feed_full);
                    } else {
                        // HashSet the image drawable to indicate that you have not liked this post
                        holder.likeImage2.setImageResource(R.drawable.ic_action_like_feed);
                    }

                    // Retrieve reply objcetId
                    string commentId = string.valueOf(reply[0].getObjectId());

                    // Click listener for liking a reply
                    holder.likeImage2.setOnClickListener(v -> {
                        v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.like_click));
                        createLikeForComment(reply[0], position, commentId);
                    });

                    // Display user profile picture
                    downloadProfilePicture1(holder, reply[0]);
                }

                holder.replyImage2.setOnClickListener(v -> {
                    bool isOnline = NetworkHelper.isOnline(mContext);
                    if (!isOnline) {
                        Toast.makeText(mContext, R.string.cannot_retrieve_messages, Toast.LENGTH_SHORT).show();
                    } else {
                        v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));
                        retrievePointerObjectIdForReply(yeet);
                    }
                });

            } else {
                Log.e("Error", e.Message);
            }
        });
    }


    private void downloadProfilePicture(ViewHolder holder, ParseObject yeet) {
        ParseQuery<ParseUser> query = ParseUser.getQuery();
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, yeet.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId());
        query.fromLocalDatastore();
        query.findInBackground((user, e) -> {
            if (e == null) for (ParseObject userObject : user) {

                if (userObject.getParseFile(ParseConstants.KEY_PROFILE_PICTURE) != null) {

                    // Pin object to local datastore
                    ParseObject.pinAllInBackground(user);

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

                holder.username.setText(userObject.getString(ParseConstants.KEY_USERNAME));

            }
        });
    }


    private void downloadProfilePicture2(ViewHolder holder, ParseObject replyObject) {
        ParseQuery<ParseUser> query = ParseUser.getQuery();
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, replyObject.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId());
        query.fromLocalDatastore();
        query.findInBackground((user, e) -> {
            if (e == null) {

                if (user[0].getParseFile("profilePicture") != null) {
                    // Pin object to local datastore
                    ParseObject.pinAllInBackground(user);

                    string profilePictureURL = user[0].getParseFile("profilePicture").getUrl();

                    // Asynchronously display the profile picture downloaded from Parse
                    if (profilePictureURL != null) {

                        Picasso.with(mContext)
                                .load(profilePictureURL)
                                //.networkPolicy(NetworkPolicy.OFFLINE)
                                .placeholder(R.color.placeholderblue)
                                .fit()
                                .into(holder.profilePicture2);

                    } else {
                        holder.profilePicture2.setImageResource(int.Parse(string.valueOf(R.drawable.ic_profile_pic_add)));
                    }
                }

                if (!(user[0].getString(ParseConstants.KEY_AUTHOR_FULL_NAME).IsEmpty())) {
                    holder.fullName2.setText(user[0].getString(ParseConstants.KEY_AUTHOR_FULL_NAME));
                } else {
                    holder.fullName2.setText(R.string.anonymous_fullName);
                }

                holder.username2.setText(user[0].getString(ParseConstants.KEY_USERNAME));

            }
        });
    }


    private void downloadProfilePicture3(ViewHolder holder, ParseObject replyObject) {
        ParseQuery<ParseUser> query = ParseUser.getQuery();
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, replyObject.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId());
        query.fromLocalDatastore();
        query.findInBackground((user, e) -> {
            if (e == null) {

                if (user[0].getParseFile("profilePicture") != null) {
                    // Pin object to local datastore
                    ParseObject.pinAllInBackground(user);

                    string profilePictureURL = user[0].getParseFile("profilePicture").getUrl();

                    // Asynchronously display the profile picture downloaded from Parse
                    if (profilePictureURL != null) {

                        Picasso.with(mContext)
                                .load(profilePictureURL)
                                //.networkPolicy(NetworkPolicy.OFFLINE)
                                .placeholder(R.color.placeholderblue)
                                .fit()
                                .into(holder.profilePicture3);

                    } else {
                        holder.profilePicture3.setImageResource(int.Parse(string.valueOf(R.drawable.ic_profile_pic_add)));
                    }
                }

                if (!(user[0].getString(ParseConstants.KEY_AUTHOR_FULL_NAME).IsEmpty())) {
                    holder.fullName3.setText(user[0].getString(ParseConstants.KEY_AUTHOR_FULL_NAME));
                } else {
                    holder.fullName3.setText(R.string.anonymous_fullName);
                }

                holder.username3.setText(user[0].getString(ParseConstants.KEY_USERNAME));

            }
        });
    }


    private void downloadProfilePicture1(ViewHolder holder, ParseObject replyObject) {
        ParseQuery<ParseUser> query = ParseUser.getQuery();
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, replyObject.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId());
        query.fromLocalDatastore();
        query.findInBackground((user, e) -> {
            if (e == null) {

                if (user[0].getParseFile("profilePicture") != null) {
                    // Pin object to local datastore
                    ParseObject.pinAllInBackground(user);

                    string profilePictureURL = user[0].getParseFile("profilePicture").getUrl();

                    // Asynchronously display the profile picture downloaded from Parse
                    if (profilePictureURL != null) {

                        Picasso.with(mContext)
                                .load(profilePictureURL)
                                //.networkPolicy(NetworkPolicy.OFFLINE)
                                .placeholder(R.color.placeholderblue)
                                .fit()
                                .into(holder.profilePicture2);

                    } else {
                        holder.profilePicture2.setImageResource(int.Parse(string.valueOf(R.drawable.ic_profile_pic_add)));
                    }
                } else {
                    holder.profilePicture2.setImageResource(int.Parse(string.valueOf(R.drawable.ic_profile_pic_add)));
                }

                if (user[0].getString(ParseConstants.KEY_AUTHOR_FULL_NAME) != null) {
                    holder.fullName2.setText(user[0].getString(ParseConstants.KEY_AUTHOR_FULL_NAME));
                } else {
                    holder.fullName2.setText(R.string.anonymous_fullName);
                }

                holder.username2.setText(user[0].getString(ParseConstants.KEY_USERNAME));

            }
        });
    }


    private void downloadMessageImage(ViewHolder holder, int position) {
        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_YEET);
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
                } else {
                    holder.messageImage.setVisibility(View.GONE);
                }

            }
        });
    }


    override public int getItemViewType(int position) {
        return position;
    }

    override public int getItemCount() {
        if (mYeets == null) {
            return 0;
        } else {
            return mYeets.Count;
        }
    }


    // A view holder for all the iterative elements in our list, i.e. username, fullName, etc.
    public class ViewHolder : RecyclerView.ViewHolder {
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

        LinearLayout pollVoteLayout;
        LinearLayout pollResultsLayout;
        TextView option1;
        TextView option2;
        TextView option3;
        TextView option4;
        TextView value1;
        TextView value2;
        TextView value3;
        TextView value4;
        TextView vote1;
        TextView vote2;
        TextView vote3;
        TextView vote4;
        TextView numVotes;
        TextView votes;

        RelativeLayout resultLayout1;
        RelativeLayout resultLayout2;
        RelativeLayout resultLayout3;
        RelativeLayout resultLayout4;

        LinearLayout moreRepliesLayout;
        TextView moreRepliesCount;
        TextView moreRepliesText;

        LinearLayout yeet2;
        TextView username2;
        TextView fullName2;
        TextView messageText2;
        ImageView messageImage2;
        TextView time2;
        ImageView profilePicture2;
        ImageView likeImage2;
        TextView likeCount2;
        ImageView premiumContent2;
        TextView premiumContentText2;
        ImageView replyImage2;
        LinearLayout messageImageLayout2;

        LinearLayout yeet3;
        TextView username3;
        TextView fullName3;
        TextView messageText3;
        ImageView messageImage3;
        TextView time3;
        ImageView profilePicture3;
        ImageView likeImage3;
        TextView likeCount3;
        ImageView premiumContent3;
        TextView premiumContentText3;
        ImageView replyImage3;
        LinearLayout messageImageLayout3;

        public ViewHolder(View itemView) {
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

            pollVoteLayout = (LinearLayout) itemView.findViewById(R.id.pollVoteLayout);
            pollResultsLayout = (LinearLayout) itemView.findViewById(R.id.pollResultsLayout);
            option1 = (TextView) itemView.findViewById(R.id.option1);
            option2 = (TextView) itemView.findViewById(R.id.option2);
            option3 = (TextView) itemView.findViewById(R.id.option3);
            option4 = (TextView) itemView.findViewById(R.id.option4);
            value1 = (TextView) itemView.findViewById(R.id.value1);
            value2 = (TextView) itemView.findViewById(R.id.value2);
            value3 = (TextView) itemView.findViewById(R.id.value3);
            value4 = (TextView) itemView.findViewById(R.id.value4);
            vote1 = (TextView) itemView.findViewById(R.id.vote1);
            vote2 = (TextView) itemView.findViewById(R.id.vote2);
            vote3 = (TextView) itemView.findViewById(R.id.vote3);
            vote4 = (TextView) itemView.findViewById(R.id.vote4);
            numVotes = (TextView) itemView.findViewById(R.id.numVotes);
            votes = (TextView) itemView.findViewById(R.id.votes);

            resultLayout1 = (RelativeLayout) itemView.findViewById(R.id.resultLayout1);
            resultLayout2 = (RelativeLayout) itemView.findViewById(R.id.resultLayout2);
            resultLayout3 = (RelativeLayout) itemView.findViewById(R.id.resultLayout3);
            resultLayout4 = (RelativeLayout) itemView.findViewById(R.id.resultLayout4);

            moreRepliesLayout = (LinearLayout) itemView.findViewById(R.id.moreRepliesLayout);
            moreRepliesCount = (TextView) itemView.findViewById(R.id.moreRepliesCount);
            moreRepliesText = (TextView) itemView.findViewById(R.id.moreRepliesText);

            yeet2 = (LinearLayout) itemView.findViewById(R.id.yeet2);
            username2 = (TextView) itemView.findViewById(R.id.username2);
            fullName2 = (TextView) itemView.findViewById(R.id.fullName2);
            messageText2 = (TextView) itemView.findViewById(R.id.messageText2);
            messageImage2 = (ImageView) itemView.findViewById(R.id.messageImage2);
            time2 = (TextView) itemView.findViewById(R.id.time2);
            profilePicture2 = (ImageView) (itemView.findViewById(R.id.profilePicture2));
            messageImageLayout2 = (LinearLayout) itemView.findViewById(R.id.messageImageLayout2);
            likeImage2 = (ImageView) itemView.findViewById(R.id.likeImage2);
            likeCount2 = (TextView) itemView.findViewById(R.id.likeCount2);
            replyImage2 = (ImageView) itemView.findViewById(R.id.replyImage2);
            premiumContent2 = (ImageView) itemView.findViewById(R.id.premiumContent2);
            premiumContentText2 = (TextView) itemView.findViewById(R.id.premiumContentText2);

            yeet3 = (LinearLayout) itemView.findViewById(R.id.yeet3);
            username3 = (TextView) itemView.findViewById(R.id.username3);
            fullName3 = (TextView) itemView.findViewById(R.id.fullName3);
            messageText3 = (TextView) itemView.findViewById(R.id.messageText3);
            messageImage3 = (ImageView) itemView.findViewById(R.id.messageImage3);
            time3 = (TextView) itemView.findViewById(R.id.time3);
            profilePicture3 = (ImageView) (itemView.findViewById(R.id.profilePicture3));
            messageImageLayout3 = (LinearLayout) itemView.findViewById(R.id.messageImageLayout3);
            likeImage3 = (ImageView) itemView.findViewById(R.id.likeImage3);
            likeCount3 = (TextView) itemView.findViewById(R.id.likeCount3);
            replyImage3 = (ImageView) itemView.findViewById(R.id.replyImage3);
            premiumContent3 = (ImageView) itemView.findViewById(R.id.premiumContent3);
            premiumContentText3 = (TextView) itemView.findViewById(R.id.premiumContentText3);
        }
    }
}
}
