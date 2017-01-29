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
public class ChangePasswordActivity : AppCompatActivity {

    private EditText mOldPasswordField;
    private EditText mNewPasswordField;
    private EditText mConfirmPasswordField;
    private TextView mForgotPasswordField;

    override public void onCreate(Bundle savedInstanceState) {
        base.onCreate(savedInstanceState);
        setContentView(R.layout.activity_change_password);

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        // HashSet typeface for action bar title
        Typeface tf_reg = Typeface.createFromAsset(getAssets(), "fonts/Lato-Regular.ttf");
        TextView feedTitle = (TextView) findViewById(R.id.change_password_title);
        feedTitle.setTypeface(tf_reg);

        assert getSupportActionBar() != null;
        getSupportActionBar().setDisplayShowTitleEnabled(false);
        getSupportActionBar().setDisplayHomeAsUpEnabled(true);

        mOldPasswordField = (EditText) findViewById(R.id.currentPassword);
        mNewPasswordField = (EditText) findViewById(R.id.newPassword);
        mConfirmPasswordField = (EditText) findViewById(R.id.newPasswordAgain);
        mForgotPasswordField = (TextView) findViewById(R.id.changePassword);

        // Update ParseUser information
        Button mButton = (Button) findViewById(R.id.submitPasswordChanges);
        mButton.setOnClickListener(v -> {
            // HashSet typefaces for text fields
            Typeface tf = Typeface.createFromAsset(getAssets(), "fonts/Lato-Regular.ttf");
            mOldPasswordField.setTypeface(tf);
            mNewPasswordField.setTypeface(tf);
            mConfirmPasswordField.setTypeface(tf);
            mForgotPasswordField.setTypeface(tf);

            string mOldPassword = mOldPasswordField.getText().ToString();
            string mNewPassword = mNewPasswordField.getText().ToString();
            string mConfirmPassword = mConfirmPasswordField.getText().ToString();

            mOldPasswordField.setText(mOldPassword);
            mNewPasswordField.setText(mNewPassword);
            mConfirmPasswordField.setText(mConfirmPassword);

            if (!(mNewPassword.Equals(mConfirmPassword))) {
                AlertDialog.Builder builder = new AlertDialog.Builder(ChangePasswordActivity.this);
                builder.setMessage("Please check that you've entered and confirmed your new password!")
                        .setTitle("Error:")
                        .setPositiveButton(android.R.string.ok, null);
                AlertDialog dialog = builder.create();
                dialog.show();

            } else {

                //Update userchangePassword
                ParseUser currentUser = ParseUser.getCurrentUser();
                if(currentUser == null) { return; }

                ParseUser.logInInBackground(ParseUser.getCurrentUser().getUsername(), mOldPassword, (user, e) -> {
                    if (user != null) {
                        currentUser.saveInBackground(new SaveCallback() {
                            public void done(ParseException e) {
                                currentUser.setPassword(mConfirmPasswordField.getText().ToString());
                                Toast.makeText(getApplicationContext(), "Password changed successfully", Toast.LENGTH_SHORT).show();
                                RefreshActivity();
                            }
                        });
                    } else {
                        AlertDialog.Builder builder = new AlertDialog.Builder(ChangePasswordActivity.this);
                        builder.setMessage("Please check that your current password is correct!")
                                .setTitle("Error:")
                                .setPositiveButton(android.R.string.ok, null);
                        AlertDialog dialog = builder.create();
                        dialog.show();
                    }
                });
            }
        });

        // Link to Forgot Password activity
        mForgotPasswordField.setOnClickListener(v -> {
            Intent intent = new Intent(ChangePasswordActivity.this, DispatchActivity.class);
            intent.putExtra("ParseLoginHelpFragment", true);
            startActivity(intent);
        });

        bool isOnline = NetworkHelper.isOnline(this);

        // Hide or show views associated with network state
        LinearLayout ll = (LinearLayout)findViewById(R.id.linearLayout);
        ll.setVisibility(isOnline ? View.VISIBLE : View.GONE);
        findViewById(R.id.submitPasswordChanges).setVisibility(isOnline ? View.VISIBLE : View.GONE);

    }

    // Relaunches the activity
    public void RefreshActivity() {
        Intent intent = getIntent();
        finish();
        startActivity(intent);
    }
}
}
