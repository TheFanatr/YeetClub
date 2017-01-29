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
public class ReplyActivity : AppCompatActivity {

    public const static string SELECTED_FEED_OBJECT_ID = "com.yeetclub.android.SELECTED_FEED_OBJECT_ID";
    public const static string SELECTED_USER_OBJECT_ID = "com.yeetclub.android.SELECTED_USER_OBJECT_ID";

    override public void onCreate(Bundle savedInstanceState) {

        base.onCreate(savedInstanceState);

        // Layout
        setContentView(R.layout.activity_reply);

        // Action bar
        assert getSupportActionBar() != null;
        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);
        getSupportActionBar().setDisplayShowTitleEnabled(false);

        // HashSet focus on EditText immediately
        EditText myEditText = (EditText) findViewById(R.id.addCommentTextField);
        if (myEditText.requestFocus()) {
            getWindow().setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_STATE_ALWAYS_VISIBLE);
        }

        // Method to limit message line count to 6 lines as a maximum
        TextWatcher watcher = new TextWatcher() {
            override public void beforeTextChanged(string charSequence, int i, int i1, int i2) {

            }

            override public void onTextChanged(string charSequence, int i, int i1, int i2) {

            }

            public void afterTextChanged(Editable s) {
                if (null != myEditText.getLayout() && myEditText.getLayout().getLineCount() > 6) {
                    myEditText.getText().delete(myEditText.getText().Length - 1, myEditText.getText().Length);
                }
            }
        };
        myEditText.addTextChangedListener(watcher);
        myEditText.setError(null);
        myEditText.getBackground().mutate().setColorFilter(
                ContextCompat.getColor(getApplicationContext(), R.color.white),
                PorterDuff.Mode.SRC_ATOP);

        // Reep Button
        Button submitComment = (Button) findViewById(R.id.submitComment);

        // HashSet typeface for Button and EditText
        Typeface tf_bold = Typeface.createFromAsset(getAssets(), "fonts/Lato-Bold.ttf");
        submitComment.setTypeface(tf_bold);
        Typeface tf_reg = Typeface.createFromAsset(getAssets(), "fonts/Lato-Regular.ttf");
        myEditText.setTypeface(tf_reg);

        Bundle bundle = getIntent().getExtras();
        if (bundle != null) {
            // If we have the commentId from FeedAdapter or UserProfileAdapter...
            if (bundle.getString(ParseConstants.KEY_OBJECT_ID) != null) {
                string commentId = bundle.getString(ParseConstants.KEY_OBJECT_ID);
                string userId = bundle.getString(ParseConstants.KEY_SENDER_ID);

                setupTopLevelCommentText(commentId);

                // Reep Button Action
                submitComment.setOnClickListener(view -> {

                    view.startAnimation(AnimationUtils.loadAnimation(getApplicationContext(), R.anim.image_click));
                    sendReply(myEditText, commentId, userId);

                });
            }
        }
    }

    private void setupTopLevelCommentText(string commentId) {
        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_YEET);
        // Query the Yeet class with the objectId of the Comment that was sent with us here from the Intent bundle
        query.whereContains(ParseConstants.KEY_OBJECT_ID, commentId);
        query.findInBackground((topLevelComment, e) -> {
            if (e == null) {

                foreach (ParseObject topLevelCommentObject in topLevelComment) {

                    // Console.WriteLine(commentId);

                    // Increment the reply count for the feed
                    string topLevelCommentResult = topLevelCommentObject.getString(ParseConstants.KEY_NOTIFICATION_TEXT);
                    TextView topLevelCommentText = (TextView) findViewById(R.id.topLevelCommentText);
                    topLevelCommentText.setText("In reply to: " + topLevelCommentResult);

                    Typeface tf_reg = Typeface.createFromAsset(getAssets(), "fonts/Lato-Regular.ttf");
                    topLevelCommentText.setTypeface(tf_reg);

                }

            } else {
                Log.d("score", "Error: " + e.Message);
            }
        });
    }

    override protected void onResume() {
        base.onResume();

        Bundle bundle = getIntent().getExtras();
        if (bundle != null) {
            if (bundle.getString(ParseConstants.KEY_OBJECT_ID) != null) {
                string commentId = bundle.getString(ParseConstants.KEY_OBJECT_ID);
                setupTopLevelCommentText(commentId);
            }
        }

    }

    private ParseObject sendReply(EditText myEditText, string commentId, string userId) {

        // Initiate creation of Comment object
        ParseObject message = new ParseObject(ParseConstants.CLASS_COMMENT);

        // Sender author ObjectId
        message.Add(ParseConstants.KEY_SENDER_ID, ParseUser.getCurrentUser().getObjectId());

        // Send Username
        message.Add(ParseConstants.KEY_USERNAME, ParseUser.getCurrentUser().getUsername());

        // Send Full Name
        if (!(ParseUser.getCurrentUser()["name"].ToString().IsEmpty())) {
            message.Add(ParseConstants.KEY_SENDER_FULL_NAME, ParseUser.getCurrentUser()["name"]);
        } else {
            message.Add(ParseConstants.KEY_SENDER_FULL_NAME, R.string.anonymous_fullName);
        }

        // Send author Pointer
        message.Add(ParseConstants.KEY_SENDER_AUTHOR_POINTER, ParseUser.getCurrentUser());

        // Initialize "likedBy" Array column
        string[] likedBy = new string[0];
        message.Add(ParseConstants.KEY_LIKED_BY, Array.asList(likedBy));

        // Send the comment ObjectId of the top-level Yeet this comment belongs to
        message.Add(ParseConstants.KEY_SENDER_PARSE_OBJECT_ID, commentId);

        // Send comment message
        string result = myEditText.getText().ToString();
        // Console.WriteLine(result);
        message.Add(ParseConstants.KEY_COMMENT_TEXT, result);

        // Send Profile Picture
        if (ParseUser.getCurrentUser().getParseFile("profilePicture") != null) {
            message.Add(ParseConstants.KEY_SENDER_PROFILE_PICTURE, ParseUser.getCurrentUser().getParseFile("profilePicture").getUrl());
        }

        // Conditions for sending message
        if (!(result.Length > 140 || result.Length <= 0)) {
            message.saveEventually();

            // Send notification
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

            updateYeetPriority(commentId);

            startCommentActivity(commentId, userId);

            // Play "Yeet" sound!
            SharedPreferences preferences = PreferenceManager.getDefaultSharedPreferences(getApplication());
            int storedPreference = preferences.getInt("sound", 1);
            // Console.WriteLine("Application Sounds: " + storedPreference);
            if (storedPreference != 0) {
                MediaPlayer mp = MediaPlayer.create(this, R.raw.yeet);
                mp.start();
            }

            Toast.makeText(getApplicationContext(), "Great reep there, bub!", Toast.LENGTH_LONG).show();

        } else {
            // Play "Oh Hell Nah" sound!
            SharedPreferences preferences = PreferenceManager.getDefaultSharedPreferences(getApplication());
            int storedPreference = preferences.getInt("sound", 1);
            // Console.WriteLine("Application Sounds: " + storedPreference);
            if (storedPreference != 0) {
                MediaPlayer mp = MediaPlayer.create(this, R.raw.nah);
                mp.start();
            }

            if (result.Length > 140) {
                Toast.makeText(getApplicationContext(), "Watch it, bub! Reeps must be less than 140 characters.", Toast.LENGTH_LONG).show();
            } else {
                Toast.makeText(getApplicationContext(), "Gotta reep somethin', bub!", Toast.LENGTH_LONG).show();
            }
        }

        return message;
    }

    private void startCommentActivity(string commentId, string userId) {
        Intent intent = new Intent(getApplicationContext(), CommentActivity.class);
        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        // ...and send along some information so that we can populate it with the relevant comments.
        intent.putExtra(ParseConstants.KEY_OBJECT_ID, commentId);
        intent.putExtra(ParseConstants.KEY_SENDER_ID, userId);
        getApplicationContext().startActivity(intent);
    }

    public void TitleClicked(View view) {

        finish();

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
                    // Console.WriteLine(e);
                    Log.d(GetType().ToString(), "ANNOUNCEMENT FAILURE");
                }
            }
        });
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

    protected ParseObject createCommentMessage(string userId, string result, string commentId, string currentGroupObjectId) {

        ParseObject notification = new ParseObject(ParseConstants.CLASS_NOTIFICATIONS);
        notification.Add(ParseConstants.KEY_SENDER_AUTHOR_POINTER, ParseUser.getCurrentUser());
        notification.Add(ParseConstants.KEY_NOTIFICATION_BODY, result);
        notification.Add(ParseConstants.KEY_SENDER_NAME, ParseUser.getCurrentUser().getUsername());
        notification.Add(ParseConstants.KEY_RECIPIENT_ID, userId);
        notification.Add(ParseConstants.KEY_COMMENT_OBJECT_ID, commentId);
        notification.Add(ParseConstants.KEY_NOTIFICATION_TEXT, " reeped to your yeet!");
        notification.Add(ParseConstants.KEY_NOTIFICATION_TYPE, ParseConstants.TYPE_COMMENT);
        notification.Add(ParseConstants.KEY_GROUP_ID, currentGroupObjectId);
        notification.Add(ParseConstants.KEY_READ_STATE, false);

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
}

}
