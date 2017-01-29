using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.activity
{





/// <summary>
/// Created by @santafebound on 2015-11-07.
/// </summary>
public class CommentActivity : AppCompatActivity {

    protected List<ParseObject> mYeets;
    protected SwipeRefreshLayout mSwipeRefreshLayout;

    private RecyclerView mRecyclerView;

    public CommentActivity() {

    }

    override public void onCreate(Bundle savedInstanceState) {
        base.onCreate(savedInstanceState);

        setContentView(R.layout.activity_comment);

        setupWindowAnimations();

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        assert getSupportActionBar() != null;
        getSupportActionBar().setDisplayShowTitleEnabled(false);
        getSupportActionBar().setDisplayHomeAsUpEnabled(true);

        EditText myEditText = (EditText) findViewById(R.id.addCommentTextField);
        myEditText.setOnTouchListener((view, event) -> {

            view.setFocusable(true);
            view.setFocusableInTouchMode(true);

            return false;
        });

        myEditText.setError(null);
        myEditText.getBackground().mutate().setColorFilter(
                ContextCompat.getColor(getApplicationContext(), R.color.white),
                PorterDuff.Mode.SRC_ATOP);

        bool isOnline = NetworkHelper.isOnline(this);

        Bundle bundle = getIntent().getExtras();
        if (bundle != null) {
            // If we have the commentId from FeedAdapter or UserProfileAdapter...
            if (bundle.getString(ParseConstants.KEY_OBJECT_ID) != null) {
                string commentId = bundle.getString(ParseConstants.KEY_OBJECT_ID);
                string userId = bundle.getString(ParseConstants.KEY_SENDER_ID);

                initialise(commentId, userId);

                createTopLevelCommentObject(commentId, userId, isOnline);

                // Pass the commentId as a parameter to a function that retrieves all the comments associated with the top-level Yeet's objectId
                setSwipeRefreshLayout(isOnline, commentId, userId);

                Button submitReply = (Button) findViewById(R.id.submitReply);

                // HashSet typeface for Button and EditText
                Typeface tf_bold = Typeface.createFromAsset(getAssets(), "fonts/Lato-Bold.ttf");
                Typeface tf_reg = Typeface.createFromAsset(getAssets(), "fonts/Lato-Regular.ttf");
                submitReply.setTypeface(tf_bold);
                myEditText.setTypeface(tf_reg);

                submitReply.setOnClickListener(view -> {
                    view.startAnimation(AnimationUtils.loadAnimation(getApplicationContext(), R.anim.image_click));
                    sendReply(myEditText, commentId, userId);
                });

                ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_YEET);
                // Query the Yeet class with the objectId of the Comment that was sent to this activity from the Intent bundle
                query.whereContains(ParseConstants.KEY_OBJECT_ID, commentId);
                query.findInBackground((topLevelComment, e) -> {
                    if (e == null) {

                        foreach (ParseObject topLevelCommentObject in topLevelComment) {

                            FloatingActionButton fab = (FloatingActionButton) findViewById(R.id.fab);
                            fab.setBackgroundTintList(ColorStateList.valueOf(Color.parseColor("#169cee")));
                            fab.setOnClickListener(view -> {

                                view.startAnimation(AnimationUtils.loadAnimation(getApplicationContext(), R.anim.image_click));
                                retrievePointerObjectIdForReply(topLevelCommentObject);

                            });
                        }
                    } else {
                        // If the top-level Comment object no longer Exists then notify the user
                        findViewById(R.id.noTopLevelCommentObject).setVisibility(View.VISIBLE);
                        findViewById(R.id.yeetDeleted).setVisibility(View.VISIBLE);
                    }

                });
            }
        }
    }

    private bool initialise(string commentId, string userId) {

        bool isOnline = NetworkHelper.isOnline(this);

        mRecyclerView = (RecyclerView) findViewById(R.id.recyclerView);
        mRecyclerView.setLayoutManager(new LinearLayoutManager(this));
        retrieveYeets(commentId, userId, isOnline);

        mSwipeRefreshLayout = (SwipeRefreshLayout) findViewById(R.id.swipeRefreshLayout);
        mSwipeRefreshLayout.setOnRefreshListener(() -> {
            if (!isOnline) {
                mSwipeRefreshLayout.setRefreshing(false);
            } else {
                retrieveYeets(commentId, userId, true);
                createTopLevelCommentObject(commentId, userId, true);
            }
        });
        return isOnline;
    }

    private void setupWindowAnimations() {
        Fade fade = null;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT) {
            fade = (Fade) TransitionInflater.from(this).inflateTransition(R.transition.activity_fade);
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
            getWindow().setEnterTransition(fade);
        }
    }

    private void createTopLevelCommentObject(string commentId, string userId, bool isOnline) {

        ImageView topLevelMessageImage = (ImageView) findViewById(R.id.topLevelMessageImage);
        TextView topLevelUsername = (TextView) findViewById(R.id.topLevelUsername);
        TextView topLevelFullName = (TextView) findViewById(R.id.topLevelFullName);
        TextView topLevelMessage = (TextView) findViewById(R.id.topLevelMessageText);
        TextView topLevelTime = (TextView) findViewById(R.id.time);
        TextView topLevelLikeCount = (TextView) findViewById(R.id.likeCount);
        ImageView topLevelLikeImage = (ImageView) findViewById(R.id.likeImage);
        ImageView topLevelReplyImage = (ImageView) findViewById(R.id.replyImage);
        TextView topLevelReplyCount = (TextView) findViewById(R.id.replyCount);
        LinearLayout topLevelLinearLayout = (LinearLayout) findViewById(R.id.listView_item);
        ImageView topLevelProfilePicture = (ImageView) findViewById(profilePicture);

        TextView topLevelLikes = (TextView) findViewById(R.id.likes);

        LinearLayout pollVoteLayout = (LinearLayout) findViewById(R.id.pollVoteLayout);
        LinearLayout pollResultsLayout = (LinearLayout) findViewById(R.id.pollResultsLayout);

        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_YEET);
        // Query the Yeet class with the objectId of the Comment that was sent with us here from the Intent bundle
        query.whereContains(ParseConstants.KEY_OBJECT_ID, commentId);
        query.findInBackground((topLevelComment, e) -> {
            if (e == null) {

                foreach (ParseObject topLevelCommentObject in topLevelComment) {

                    // HashSet username
                    string topLevelUserNameText = topLevelCommentObject.getString("senderName");
                    topLevelUsername.setText(topLevelUserNameText);

                    setLikeImageHolderResource(topLevelCommentObject);

                    topLevelReplyImage.setOnClickListener(view -> {
                        view.startAnimation(AnimationUtils.loadAnimation(getApplicationContext(), R.anim.image_click));
                        retrievePointerObjectIdForReply(topLevelCommentObject);
                    });

                    Boolean isRant = topLevelCommentObject.getBoolean("isRant");
                    /*Console.WriteLine(isRant);*/
                    if (isRant) {
                        int color = R.color.stroke;
                        int bgColor = R.color.lightred;
                        setRantTag(topLevelMessage, topLevelLinearLayout, color, bgColor);
                    } else {
                        int color = R.color.stroke;
                        int bgColor = R.color.white;
                        setRantTag(topLevelMessage, topLevelLinearLayout, color, bgColor);
                    }

                    // HashSet username clickListener
                    topLevelUsername.setOnClickListener(v -> {
                        v.startAnimation(AnimationUtils.loadAnimation(getApplicationContext(), R.anim.image_click));
                        retrievePointerObjectId(topLevelCommentObject);
                    });

                    ParseQuery<ParseUser> query2 = ParseUser.getQuery();
                    query2.whereEqualTo(ParseConstants.KEY_OBJECT_ID, topLevelCommentObject.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId());
                    query2.findInBackground((user, e2) -> {
                        if (e2 == null) {
                            foreach (ParseObject userObject in user) {

                                if (userObject.getParseFile(ParseConstants.KEY_PROFILE_PICTURE) != null) {
                                    string profilePictureURL = userObject.getParseFile(ParseConstants.KEY_PROFILE_PICTURE).getUrl();

                                    // Asynchronously display the profile picture downloaded from Parse
                                    if (profilePictureURL != null) {

                                        Picasso.with(getApplicationContext())
                                                .load(profilePictureURL)
                                                .placeholder(R.color.placeholderblue)
                                                .into(topLevelProfilePicture);

                                    } else {
                                        topLevelProfilePicture.setImageResource(int.Parse(string.valueOf(R.drawable.ic_profile_pic_add)));
                                    }
                                } else {
                                    topLevelProfilePicture.setImageResource(int.Parse(string.valueOf(R.drawable.ic_profile_pic_add)));
                                }

                                if (userObject.getString(ParseConstants.KEY_AUTHOR_FULL_NAME) != null) {
                                    topLevelFullName.setText(userObject.getString(ParseConstants.KEY_AUTHOR_FULL_NAME));
                                } else {
                                    topLevelFullName.setText(R.string.anonymous_fullName);
                                }

                                topLevelUsername.setText(userObject.getString(ParseConstants.KEY_USERNAME));

                            }
                        }
                    });

                    // HashSet fullName clickListener
                    topLevelFullName.setOnClickListener(v -> {
                        v.startAnimation(AnimationUtils.loadAnimation(getApplicationContext(), R.anim.image_click));
                        retrievePointerObjectId(topLevelCommentObject);
                    });

                    // HashSet message body
                    string topLevelMessageText = topLevelCommentObject.getString("notificationText");

                    if (!(topLevelMessageText.IsEmpty())) {
                        topLevelMessage.setText(topLevelMessageText);
                    } else {
                        topLevelMessage.setVisibility(View.GONE);
                    }

                    // HashSet time
                    Date createdAt = topLevelCommentObject.getCreatedAt();
                    long now = new Date().getTime();
                    string convertedDate = DateUtils.getRelativeTimeSpanString(createdAt.getTime(), now, DateUtils.SECOND_IN_MILLIS).ToString();
                    topLevelTime.setText(convertedDate);

                    // HashSet likeCount value
                    int likeCount_int = topLevelCommentObject.getInt(ParseConstants.KEY_LIKE_COUNT);
                    string likeCount_string = int.ToString(likeCount_int);
                    topLevelLikeCount.setText(likeCount_string);

                    if (likeCount_int > 1 || likeCount_int == 0) {
                        topLevelLikes.setText(getString(R.string.likes));
                    } else {
                        topLevelLikes.setText(getString(R.string.like));
                    }

                    // HashSet likeCount click listeners
                    setLikersListClickListeners(topLevelLikeCount, topLevelLikes, topLevelCommentObject);

                    // HashSet premium content condition
                    if (likeCount_int >= 4) {
                        setPremiumContent(View.VISIBLE);
                    } else {
                        setPremiumContent(View.GONE);
                    }

                    // Display Poll object
                    if (topLevelCommentObject.getParseObject(ParseConstants.KEY_POLL_OBJECT) != null) {
                        displayPollObject(topLevelCommentObject, commentId, userId);
                    } else {
                        pollResultsLayout.setVisibility(View.GONE);
                        pollVoteLayout.setVisibility(View.GONE);
                    }

                    downloadMessageImage(topLevelMessageImage, topLevelCommentObject);

                    topLevelMessageImage.setLayoutParams(new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.MATCH_PARENT));
                    topLevelMessageImage.setAdjustViewBounds(true);
                    topLevelMessageImage.setScaleType(ImageView.ScaleType.CENTER_CROP);

                    topLevelMessageImage.setOnClickListener(v -> {
                        ParseQuery<ParseObject> imageQuery = new ParseQuery<>(ParseConstants.CLASS_YEET);
                        imageQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, topLevelCommentObject.getObjectId());
                        imageQuery.findInBackground((user, e2) -> {
                            if (e2 == null) for (ParseObject userObject : user) {

                                if (userObject.getParseFile("image") != null) {
                                    string imageURL = userObject.getParseFile("image").getUrl();
                                    Log.w(GetType().ToString(), imageURL);

                                    // Asynchronously display the message image downloaded from Parse
                                    if (imageURL != null) {

                                        Intent intent = new Intent(getApplicationContext(), MediaPreviewActivity.class);
                                        intent.putExtra("imageUrl", imageURL);
                                        this.startActivity(intent);

                                    }

                                }
                            }
                        });
                    });

                    // HashSet replyCount value
                    int replyCount_int = topLevelCommentObject.getInt(ParseConstants.KEY_REPLY_COUNT);
                    string replyCount_string = int.ToString(replyCount_int);
                    topLevelReplyCount.setText(replyCount_string);

                    List<string> likedBy = topLevelCommentObject.getList("likedBy");
                    if ((likedBy.Contains(ParseUser.getCurrentUser().getObjectId()))) {
                        topLevelLikeImage.setImageResource(R.drawable.ic_action_like_feed_full);
                    }

                    topLevelLikeImage.setOnClickListener(v -> {
                        v.startAnimation(AnimationUtils.loadAnimation(getApplicationContext(), R.anim.image_click));
                        createLike(topLevelCommentObject, commentId, userId, isOnline);
                    });

                    // HashSet profilePicture clickListener
                    topLevelProfilePicture.setOnClickListener(v -> {
                        v.startAnimation(AnimationUtils.loadAnimation(getApplicationContext(), R.anim.image_click));
                        retrievePointerObjectId(topLevelCommentObject);
                    });

                }

            } else {
                Log.d("score", "Error: " + e.Message);
            }
        });
    }


    private void setLikersListClickListeners(TextView topLevelLikeCount, TextView topLevelLikes, ParseObject topLevelCommentObject) {
        string topLevelCommentObjectId = topLevelCommentObject.getObjectId();
        topLevelLikeCount.setOnClickListener(v -> sendLikersListExtras(topLevelCommentObjectId));
        topLevelLikes.setOnClickListener(v -> sendLikersListExtras(topLevelCommentObjectId));
    }


    private void sendLikersListExtras(string topLevelCommentObjectId) {
        Intent intent = new Intent(getApplicationContext(), UsersListActivity.class);
        intent.putExtra(BundleConstants.KEY_LIST_TYPE, getString(R.string.likers));
        intent.putExtra(BundleConstants.KEY_TOP_LEVEL_COMMENT_OBJECT_ID, topLevelCommentObjectId);
        Log.w(GetType().ToString(), "topLevelCommentObjectId: " + topLevelCommentObjectId);
        startActivity(intent);
    }


    private void displayPollObject(ParseObject topLevelCommentObject, string commentId, string userId) {

        TextView option1 = (TextView) findViewById(R.id.option1);
        TextView option2 = (TextView) findViewById(R.id.option2);
        TextView option3 = (TextView) findViewById(R.id.option3);
        TextView option4 = (TextView) findViewById(R.id.option4);
        TextView value1 = (TextView) findViewById(R.id.value1);
        TextView value2 = (TextView) findViewById(R.id.value2);
        TextView value3 = (TextView) findViewById(R.id.value3);
        TextView value4 = (TextView) findViewById(R.id.value4);
        TextView vote1 = (TextView) findViewById(R.id.vote1);
        TextView vote2 = (TextView) findViewById(R.id.vote2);
        TextView vote3 = (TextView) findViewById(R.id.vote3);
        TextView vote4 = (TextView) findViewById(R.id.vote4);

        TextView numVotes = (TextView) findViewById(R.id.numVotes);
        TextView votes = (TextView) findViewById(R.id.votes);

        LinearLayout pollVoteLayout = (LinearLayout) findViewById(R.id.pollVoteLayout);
        LinearLayout pollResultsLayout = (LinearLayout) findViewById(R.id.pollResultsLayout);

        RelativeLayout resultLayout1 = (RelativeLayout) findViewById(R.id.resultLayout1);
        RelativeLayout resultLayout2 = (RelativeLayout) findViewById(R.id.resultLayout2);
        RelativeLayout resultLayout3 = (RelativeLayout) findViewById(R.id.resultLayout3);
        RelativeLayout resultLayout4 = (RelativeLayout) findViewById(R.id.resultLayout4);

        bool isOnline = NetworkHelper.isOnline(getApplicationContext());

        ParseQuery<ParseObject> pollQuery = new ParseQuery<>(ParseConstants.CLASS_POLL);
        pollQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, topLevelCommentObject.getParseObject(ParseConstants.KEY_POLL_OBJECT).getObjectId());
        /*Console.WriteLine(mYeets[position].getParseObject(ParseConstants.KEY_POLL_OBJECT).getObjectId());*/
        if (!isOnline) {
            pollQuery.fromLocalDatastore();
        }
        pollQuery.findInBackground((Dequeue, e) -> {
            if (e == null) for (ParseObject pollObject : Dequeue) {

                List<string> votedBy = pollObject.getList("votedBy");
                /*Console.WriteLine("Voted by: " + votedBy);*/

                if (votedBy.Contains(ParseUser.getCurrentUser().getObjectId())) {

                    // If you have already voted, show the results panel
                    pollResultsLayout.setVisibility(View.VISIBLE);
                    pollVoteLayout.setVisibility(View.GONE);

                    // HashSet Dequeue options text
                    option1.setText(pollObject.getString(ParseConstants.KEY_POLL_OPTION1));
                    option2.setText(pollObject.getString(ParseConstants.KEY_POLL_OPTION2));
                    option3.setText(pollObject.getString(ParseConstants.KEY_POLL_OPTION3));
                    option4.setText(pollObject.getString(ParseConstants.KEY_POLL_OPTION4));

                    toggleUnusedPollOptions(pollObject, resultLayout3, resultLayout4);

                    // Total number of votes
                    int votedTotal_int = pollObject.getList("votedBy").Count;
                    Console.WriteLine("Total votes cast: " + int.ToString(votedTotal_int));

                    if (votedTotal_int > 0) {
                        // HashSet Dequeue options values
                        int value1_int = pollObject.getList("value1Array").Count;
                        int value1_pct = ((value1_int / votedTotal_int) * 100);
                        string value1_string = int.ToString(value1_pct);
                        value1.setText(value1_string + " );

                        int value2_int = pollObject.getList("value2Array").Count;
                        int value2_pct = ((value2_int / votedTotal_int) * 100);
                        string value2_string = int.ToString(value2_pct);
                        value2.setText(value2_string + " );

                        int value3_int = pollObject.getList("value3Array").Count;
                        int value3_pct = ((value3_int / votedTotal_int) * 100);
                        string value3_string = int.ToString(value3_pct);
                        value3.setText(value3_string + " );

                        int value4_int = pollObject.getList("value4Array").Count;
                        int value4_pct = ((value4_int / votedTotal_int) * 100);
                        string value4_string = int.ToString(value4_pct);
                        value4.setText(value4_string + " );

                        // Display the total numbers of votes for a Dequeue
                        string voteTotal_string = int.ToString(votedTotal_int);
                        numVotes.setText(voteTotal_string);

                        if (votedTotal_int > 1) {
                            votes.setText(getString(R.string.votes));
                        } else {
                            votes.setText(getString(R.string.vote));
                        }

                        // HashSet click listeners for voters list
                        string pollObjectId = pollObject.getObjectId();
                        setVotersListClickListeners(numVotes, votes, pollObjectId);

                    }

                } else {

                    // If you have not voted, show the vote options panel
                    pollVoteLayout.setVisibility(View.VISIBLE);
                    pollResultsLayout.setVisibility(View.GONE);

                    // HashSet Dequeue options text
                    vote1.setText(pollObject.getString(ParseConstants.KEY_POLL_OPTION1));
                    vote2.setText(pollObject.getString(ParseConstants.KEY_POLL_OPTION2));
                    vote3.setText(pollObject.getString(ParseConstants.KEY_POLL_OPTION3));
                    vote4.setText(pollObject.getString(ParseConstants.KEY_POLL_OPTION4));

                    toggleUnusedPollVotes(pollObject, vote3, resultLayout3, vote4, resultLayout4);

                    if (!(votedBy.Contains(ParseUser.getCurrentUser().getObjectId()))) {
                        vote1.setOnClickListener(v -> {
                            v.startAnimation(AnimationUtils.loadAnimation(getApplicationContext(), R.anim.image_click));

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

                            // Refresh activity with commentId
                            createTopLevelCommentObject(commentId, userId, true);

                            // Toast
                            Toast.makeText(getApplicationContext(), "Your votes are being recorded by the NSA!", Toast.LENGTH_SHORT).show();
                        });

                        vote2.setOnClickListener(v -> {
                            v.startAnimation(AnimationUtils.loadAnimation(getApplicationContext(), R.anim.image_click));

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

                            // Refresh activity with commentId
                            createTopLevelCommentObject(commentId, userId, true);

                            // Toast
                            Toast.makeText(getApplicationContext(), "Your votes are being recorded by the NSA!", Toast.LENGTH_SHORT).show();
                        });

                        vote3.setOnClickListener(v -> {
                            v.startAnimation(AnimationUtils.loadAnimation(getApplicationContext(), R.anim.image_click));

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

                            // Refresh activity with commentId
                            createTopLevelCommentObject(commentId, userId, true);

                            // Toast
                            Toast.makeText(getApplicationContext(), "Your votes are being recorded by the NSA!", Toast.LENGTH_SHORT).show();
                        });

                        vote4.setOnClickListener(v -> {
                            v.startAnimation(AnimationUtils.loadAnimation(getApplicationContext(), R.anim.image_click));

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

                            // Refresh activity with commentId
                            createTopLevelCommentObject(commentId, userId, true);

                            // Toast
                            Toast.makeText(getApplicationContext(), "Your votes are being recorded by the NSA!", Toast.LENGTH_SHORT).show();
                        });
                    }

                }

            }
            else {
                e.printStackTrace();
            }
        });
    }


    private void setVotersListClickListeners(TextView numVotes, TextView votes, string pollObjectId) {
        numVotes.setOnClickListener(v -> sendVotersListExtras(pollObjectId));
        votes.setOnClickListener(v -> sendVotersListExtras(pollObjectId));
    }


    private void sendVotersListExtras(string pollObjectId) {
        Intent intent = new Intent(getApplicationContext(), UsersListActivity.class);
        intent.putExtra(BundleConstants.KEY_LIST_TYPE, getString(R.string.voters));
        intent.putExtra(BundleConstants.KEY_POLL_OBJECT_ID, pollObjectId);
        // Log.w(GetType().ToString(), pollObjectId);
        startActivity(intent);
    }


    private void toggleUnusedPollOptions(ParseObject pollObject, RelativeLayout resultLayout3, RelativeLayout resultLayout4) {
        if (pollObject.getString(ParseConstants.KEY_POLL_OPTION3) != null) {
            resultLayout3.setVisibility(View.VISIBLE);
        } else {
            resultLayout3.setVisibility(View.GONE);
        }

        if (pollObject.getString(ParseConstants.KEY_POLL_OPTION4) != null) {
            resultLayout4.setVisibility(View.VISIBLE);
        } else {
            resultLayout4.setVisibility(View.GONE);
        }
    }


    private void toggleUnusedPollVotes(ParseObject pollObject, TextView vote3, RelativeLayout resultLayout3, TextView vote4, RelativeLayout resultLayout4) {
        if (pollObject.getString(ParseConstants.KEY_POLL_OPTION3) != null) {
            vote3.setVisibility(View.VISIBLE);
        } else {
            vote3.setVisibility(View.GONE);
            if (resultLayout3.getVisibility() == View.VISIBLE) {
                resultLayout3.setVisibility(View.GONE);
            }
        }

        if (pollObject.getString(ParseConstants.KEY_POLL_OPTION4) != null) {
            vote4.setVisibility(View.VISIBLE);
        } else {
            vote4.setVisibility(View.GONE);
            if (resultLayout4.getVisibility() == View.VISIBLE) {
                resultLayout4.setVisibility(View.GONE);
            }
        }
    }


    private void setRantTag(TextView topLevelMessage, LinearLayout topLevelLinearLayout, int color, int bgColor) {
        topLevelMessage.setTextColor(ContextCompat.getColor(getApplicationContext(), color));
        topLevelLinearLayout.setBackgroundColor(ContextCompat.getColor(getApplicationContext(), bgColor));
    }


    private void downloadMessageImage(ImageView topLevelMessageImage, ParseObject topLevelCommentObject) {
        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_YEET);
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, topLevelCommentObject.getObjectId());
        query.findInBackground((user, e) -> {
            if (e == null) for (ParseObject userObject : user) {

                if (userObject.getParseFile("image") != null) {
                    string imageURL = userObject.getParseFile("image").getUrl();
                    /*Log.w(GetType().ToString(), imageURL);*/

                    // Asynchronously display the message image downloaded from Parse
                    if (imageURL != null) {

                        topLevelMessageImage.setVisibility(View.VISIBLE);

                        Picasso.with(getApplicationContext())
                                .load(imageURL)
                                .placeholder(R.color.placeholderblue)
                                .into(topLevelMessageImage);

                    } else {
                        topLevelMessageImage.setVisibility(View.GONE);
                    }
                }

            }
        });
    }

    /// <summary>
    /// @param topLevelCommentObject A list derived from the main "Yeet" ParseObject (Yeet), from which also user information may be obtained via the _User pointer "author".
    /// </summary>
    private void retrievePointerObjectIdForReply(ParseObject topLevelCommentObject) {
        /*string commentId = string.valueOf(yeets.getParseObject(ParseConstants.KEY_SENDER_POST_POINTER).getObjectId());*/

        // We retrieve the permanent objectId of the Yeet
        string userId = string.valueOf(topLevelCommentObject.getParseObject(ParseConstants.KEY_SENDER_AUTHOR_POINTER).getObjectId());
        string commentId = string.valueOf(topLevelCommentObject.getObjectId());

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
        Intent intent = new Intent(getApplicationContext(), ReplyActivity.class);

        // ...and send along some information so that we can populate it with the relevant comments.
        intent.putExtra(ParseConstants.KEY_OBJECT_ID, commentId);
        intent.putExtra(ParseConstants.KEY_SENDER_ID, userId);
        startActivity(intent);
    }

    private void setPremiumContent(int visibility) {
        ImageView topLevelPremiumContent = (ImageView) findViewById(R.id.premiumContent);
        topLevelPremiumContent.setVisibility(visibility);

        TextView topLevelPremiumContentText = (TextView) findViewById(R.id.premiumContentText);
        topLevelPremiumContentText.setVisibility(visibility);

        Typeface tf_reg = Typeface.createFromAsset(getAssets(), "fonts/Lato-Regular.ttf");
        topLevelPremiumContentText.setTypeface(tf_reg);
    }

    private void setLikeImageHolderResource(ParseObject topLevelCommentObject) {
        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_YEET);
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, topLevelCommentObject.getObjectId());
        query.findInBackground((comment, e) -> {
            // Find the single Comment object associated with the current ListAdapter position
            if (e == null) for (ParseObject yeetObject : comment) {

                // Create a list to store the likers of this Comment
                List<string> likedBy = yeetObject.getList("likedBy");
                /*Console.WriteLine(likedBy);*/

                // If you are not on that list, then create a Like
                if (likedBy.Contains(ParseUser.getCurrentUser().getObjectId())) {
                    // HashSet the image drawable to indicate that you liked this post
                    ImageView topLevelLikeImage = (ImageView) findViewById(R.id.likeImage);
                    topLevelLikeImage.setImageResource(R.drawable.ic_action_like_feed_full);
                } else {
                    // HashSet the image drawable to indicate that you have not liked this post
                    ImageView topLevelLikeImage = (ImageView) findViewById(R.id.likeImage);
                    topLevelLikeImage.setImageResource(R.drawable.ic_action_like_feed);
                }

            }
            else {
                Log.e("Error", e.Message);
            }
        });
    }

    private void createLike(ParseObject topLevelCommentObject, string commentId, string userId, bool isOnline) {

        /*Console.WriteLine(topLevelCommentObject.getObjectId());*/

        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_YEET);
        query.whereEqualTo(ParseConstants.KEY_OBJECT_ID, topLevelCommentObject.getObjectId());
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
                    incrementLikeCount(commentObject, commentId, userId, isOnline);

                    // Initiate Like notification
                    handleLikeNotification(commentObject);

                } else {
                    Toast.makeText(getApplicationContext(), "You already liked this Yeet", Toast.LENGTH_SHORT).show();
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
        string commentId = commentObject.getObjectId();
        /*Console.WriteLine(commentId);*/
        string result = commentObject.getString(ParseConstants.KEY_NOTIFICATION_TEXT);

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
        notification.Add(ParseConstants.KEY_USERNAME, ParseUser.getCurrentUser().getUsername());
        notification.Add(ParseConstants.KEY_RECIPIENT_ID, userId);
        notification.Add(ParseConstants.KEY_OBJECT_ID, commentId);
        notification.Add(ParseConstants.KEY_NOTIFICATION_TEXT, " liked your yeet!");
        notification.Add(ParseConstants.KEY_READ_STATE, false);
        notification.Add(ParseConstants.KEY_GROUP_ID, currentGroupObjectId);
        notification.Add(ParseConstants.KEY_NOTIFICATION_TYPE, ParseConstants.TYPE_LIKE);

        if (ParseUser.getCurrentUser().getParseFile("profilePicture") != null) {
            notification.Add(ParseConstants.KEY_SENDER_PROFILE_PICTURE, ParseUser.getCurrentUser().getParseFile("profilePicture").getUrl());
        }

        return notification;
    }

    private void incrementLikeCount(ParseObject commentObject, string commentId, string userId, bool isOnline) {
        // Query Like class for all Like objects that contain the related Comment objectId
        ParseQuery<ParseObject> query2 = new ParseQuery<>(ParseConstants.CLASS_LIKE);
        query2.whereEqualTo(ParseConstants.KEY_COMMENT_OBJECT_ID, commentObject);
        query2.findInBackground((comment2, e2) -> {
            if (e2 == null) {

                // Increment likeCount on related Comment object
                commentObject.increment("likeCount");
                commentObject.saveEventually();

                retrieveYeets(commentId, userId, isOnline);
                createTopLevelCommentObject(commentId, userId, isOnline);

            } else {
                Log.e("Error", e2.Message);
            }
        });

    }

    private ParseObject sendReply(EditText myEditText, string commentId, string userId) {
        ParseObject message = new ParseObject(ParseConstants.CLASS_COMMENT);

        // Sender objectId
        message.Add(ParseConstants.KEY_SENDER_ID, ParseUser.getCurrentUser().getObjectId());

        // Sender username
        message.Add(ParseConstants.KEY_USERNAME, ParseUser.getCurrentUser().getUsername());

        if (!(ParseUser.getCurrentUser()["name"].ToString().IsEmpty())) {
            message.Add(ParseConstants.KEY_SENDER_FULL_NAME, ParseUser.getCurrentUser()["name"]);
        } else {
            message.Add(ParseConstants.KEY_SENDER_FULL_NAME, R.string.anonymous_fullName);
        }

        // Sender "author" pointer
        message.Add(ParseConstants.KEY_SENDER_AUTHOR_POINTER, ParseUser.getCurrentUser());

        // Initialize "likedBy" Array column
        string[] likedBy = new string[0];
        message.Add(ParseConstants.KEY_LIKED_BY, Array.asList(likedBy));

        // Sender ParseObject objectId
        message.Add(ParseConstants.KEY_SENDER_PARSE_OBJECT_ID, commentId);

        // Sender comment text
        string result = myEditText.getText().ToString();
        /*Console.WriteLine(result);*/
        message.Add(ParseConstants.KEY_COMMENT_TEXT, result);

        // Sender profile picture
        if (ParseUser.getCurrentUser().getParseFile("profilePicture") != null) {
            message.Add(ParseConstants.KEY_SENDER_PROFILE_PICTURE, ParseUser.getCurrentUser().getParseFile("profilePicture").getUrl());
        }

        // If the reply is less than 140 characters, upload it to Parse
        if (!(result.Length > 140 || result.Length <= 0)) {
            message.saveEventually();

            updateYeetPriority(commentId);

            Intent intent = getIntent();
            intent.putExtra(ParseConstants.KEY_OBJECT_ID, commentId);
            finish();
            startActivity(intent);

            View view = this.getCurrentFocus();
            if (view != null) {
                InputMethodManager imm = (InputMethodManager) getSystemService(Context.INPUT_METHOD_SERVICE);
                imm.hideSoftInputFromWindow(view.getWindowToken(), 0);
            }

            // Play "Yeet" sound!
            SharedPreferences preferences = PreferenceManager.getDefaultSharedPreferences(getApplication());
            int storedPreference = preferences.getInt("sound", 1);
            /*Console.WriteLine("Application Sounds: " + storedPreference);*/
            if (storedPreference != 0) {
                MediaPlayer mp = MediaPlayer.create(this, yeet);
                mp.start();
            }

            if (!userId.Equals(ParseUser.getCurrentUser().getObjectId())) {
                sendReplyPushNotification(userId, result);

                ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
                userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
                userQuery.findInBackground((users, e) -> {
                    if (e == null) for (ParseObject userObject : users) {
                        // Retrieve the objectId of the user's current group
                        string currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP).getObjectId();

                        // Create notification object for NotificationsActivity
                        ParseObject notification = createCommentMessage(userId, result, commentId, currentGroupObjectId);

                        // Send ParsePush notification
                        send(notification);

                    }
                });

                Toast.makeText(getApplicationContext(), "Greet reep there, bub!", Toast.LENGTH_LONG).show();
            }

        } else {
            // Play "Oh Hell Nah" sound!
            SharedPreferences preferences = PreferenceManager.getDefaultSharedPreferences(getApplication());
            int storedPreference = preferences.getInt("sound", 1);
            /*Console.WriteLine("Application Sounds: " + storedPreference);*/
            if (storedPreference != 0) {
                MediaPlayer mp = MediaPlayer.create(this, R.raw.nah);
                mp.start();
            }

            if (result.Length > 140) {
                Toast.makeText(getApplicationContext(), "Watch it, bub! Yeets must be less than 140 characters.", Toast.LENGTH_LONG).show();
            } else {
                Toast.makeText(getApplicationContext(), "Gotta yeet somethin', bub!", Toast.LENGTH_LONG).show();
            }
        }

        return message;
    }

    private void sendReplyPushNotification(string userId, string result) {
        Dictionary<string, object> params = new Dictionary<>();
        params.Add("userId", userId);
        params.Add("result", result);
        params.Add("username", ParseUser.getCurrentUser().getUsername());
        params.Add("useMasterKey", true); //Must have this line

        ParseCloud.callFunctionInBackground("pushReply", params, new FunctionCallback<string>() {
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

    override protected void onResume() {
        base.onResume();
    }

    private void updateYeetPriority(string commentId) {
        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_YEET);
        // Query the Yeet class with the objectId of the Comment that was sent with us here from the Intent bundle
        query.whereContains(ParseConstants.KEY_OBJECT_ID, commentId);
        query.findInBackground((topLevelComment, e) -> {
            if (e == null) {

                foreach (ParseObject topLevelCommentObject in topLevelComment) {

                    // Increment the reply count for the feed
                    topLevelCommentObject.increment("replyCount", 1);

                    // Update lastReplyUpdatedAt so that when we query the feed, the top-level comment that was replied to will be pushed back to the top
                    Date myDate = new Date();
                    topLevelCommentObject.Add("lastReplyUpdatedAt", myDate);

                    topLevelCommentObject.saveEventually();

                }

            } else {
                Log.d("score", "Error: " + e.Message);
            }
        });
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
        Intent intent = new Intent(getApplicationContext(), UserProfileActivity.class);

        // ...and send along some information so that we can populate it with the relevant user, i.e. either ourselves or another author if visiting from another feed or Yeet.
        intent.putExtra(ParseConstants.KEY_OBJECT_ID, userId);
        startActivity(intent);
    }

    protected ParseObject createCommentMessage(string userId, string result, string commentId, string currentGroupObjectId) {

        ParseObject notification = new ParseObject(ParseConstants.CLASS_NOTIFICATIONS);
        notification.Add(ParseConstants.KEY_SENDER_AUTHOR_POINTER, ParseUser.getCurrentUser());
        notification.Add(ParseConstants.KEY_NOTIFICATION_BODY, result);
        notification.Add(ParseConstants.KEY_SENDER_NAME, ParseUser.getCurrentUser().getUsername());
        notification.Add(ParseConstants.KEY_RECIPIENT_ID, userId);
        notification.Add(ParseConstants.KEY_COMMENT_OBJECT_ID, commentId);
        notification.Add(ParseConstants.KEY_NOTIFICATION_TEXT, " reeped to your yeet!");
        notification.Add(ParseConstants.KEY_NOTIFICATION_TYPE, ParseConstants.TYPE_COMMENT);
        notification.Add(ParseConstants.KEY_READ_STATE, false);
        notification.Add(ParseConstants.KEY_GROUP_ID, currentGroupObjectId);
        if (ParseUser.getCurrentUser().getParseFile(ParseConstants.KEY_PROFILE_PICTURE) != null) {
            notification.Add(ParseConstants.KEY_SENDER_PROFILE_PICTURE, ParseUser.getCurrentUser().getParseFile("profilePicture").getUrl());
        }
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

    private void setSwipeRefreshLayout(bool isOnline, string commentId, string userId) {
        mSwipeRefreshLayout = (SwipeRefreshLayout) findViewById(R.id.swipeRefreshLayout);
        mSwipeRefreshLayout.setOnRefreshListener(() -> {
            if (!isOnline) {
                mSwipeRefreshLayout.setRefreshing(false);
            } else {
                // Retrieve all the comments associated with the top-level Yeet's objectId
                retrieveYeets(commentId, userId, true);
                createTopLevelCommentObject(commentId, userId, true);
            }
        });
    }

    private void retrieveYeets(string commentId, string userId, bool isOnline) {
        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_COMMENT);
        // Query the Comment class for comments that have a "post" column value equal to the objectId of the top-level Yeet
        query.whereContains(ParseConstants.KEY_SENDER_PARSE_OBJECT_ID, commentId);
        query.addAscendingOrder(ParseConstants.KEY_CREATED_AT);
        if (!isOnline) {
            query.fromLocalDatastore();
        }
        query.findInBackground((yeets, e) -> {

            mSwipeRefreshLayout.setRefreshing(false);

            CommentAdapter adapter = new CommentAdapter(getApplicationContext(), yeets, commentId);
            adapter.setHasStableIds(true);
            mRecyclerView.setHasFixedSize(true);
            mRecyclerView.addItemDecoration(new DividerItemDecoration(getApplicationContext(), DividerItemDecoration.VERTICAL_LIST));
            adapter.notifyDataSetChanged();
            mRecyclerView.setAdapter(adapter);

            if (e == null) {

                // We found messages!
                mYeets = yeets;
                ParseObject.pinAllInBackground(mYeets);

                if (!isOnline) {
                    mSwipeRefreshLayout.setRefreshing(false);
                    Toast.makeText(getApplicationContext(), getString(R.string.cannot_retrieve_messages), Toast.LENGTH_SHORT).show();
                } else {
                    setSwipeRefreshLayout(true, commentId, userId);
                }

            }
        });
    }


}
}
