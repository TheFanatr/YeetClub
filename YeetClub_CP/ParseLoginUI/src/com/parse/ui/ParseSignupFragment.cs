/*
 *  Copyright (c) 2014, Parse, LLC. All rights reserved.
 *
 *  You are hereby granted a non-exclusive, worldwide, royalty-free license to use,
 *  copy, modify, and distribute this software in source code or binary form for use
 *  in connection with the web services and APIs provided by Parse.
 *
 *  As with any software that integrates with the Parse platform, your use of
 *  this software is subject to the Parse Terms of Service
 *  [https://www.parse.com/about/terms]. This copyright notice shall be
 *  included in all copies or substantial portions of the software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 *  FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 *  COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
 *  IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 *  CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.parse.ui
{




/// <summary>
/// Fragment for the user signup screen.
/// </summary>
public class ParseSignupFragment : ParseLoginFragmentBase : OnClickListener {
    public const string USERNAME = "com.parse.ui.ParseSignupFragment.USERNAME";
    public const string PASSWORD = "com.parse.ui.ParseSignupFragment.PASSWORD";

    private EditText usernameField;
    private EditText passwordField;
    private EditText confirmPasswordField;
    private EditText emailField;
    private Spinner genderField;
    //  private EditText nameField;
    private Button createAccountButton;
    private ParseOnLoginSuccessListener onLoginSuccessListener;

    private ParseLoginConfig config;
    private int minPasswordLength;

    private const string LOG_TAG = "ParseSignupFragment";
    private const int DEFAULT_MIN_PASSWORD_LENGTH = 6;
    private const string USER_OBJECT_NAME_FIELD = "name";

    public static ParseSignupFragment newInstance(Bundle configOptions, string username, string password) {
        ParseSignupFragment signupFragment = new ParseSignupFragment();
        Bundle args = new Bundle(configOptions);
        args.putString(ParseSignupFragment.USERNAME, username);
        args.putString(ParseSignupFragment.PASSWORD, password);
        signupFragment.setArguments(args);
        return signupFragment;
    }

    Spinner spinner;
    ArrayAdapter<string> adapter;

    override public View onCreateView(LayoutInflater inflater, ViewGroup parent,
                             Bundle savedInstanceState) {

        Bundle args = getArguments();
        config = ParseLoginConfig.fromBundle(args, getActivity());

        minPasswordLength = DEFAULT_MIN_PASSWORD_LENGTH;
        if (config.getParseSignupMinPasswordLength() != null) {
            minPasswordLength = config.getParseSignupMinPasswordLength();
        }

        string username = args.getString(USERNAME);
        string password = args.getString(PASSWORD);

        View v = inflater.inflate(R.layout.com_parse_ui_parse_signup_fragment,
                parent, false);

        usernameField = (EditText) v.findViewById(R.id.signup_username_input);
        usernameField.setInputType(InputType.TYPE_TEXT_VARIATION_VISIBLE_PASSWORD | InputType.TYPE_TEXT_FLAG_NO_SUGGESTIONS);
        passwordField = (EditText) v.findViewById(R.id.signup_password_input);
        confirmPasswordField = (EditText) v
                .findViewById(R.id.signup_confirm_password_input);

        Typeface tf_reg = Typeface.createFromAsset(getContext().getAssets(), "fonts/Lato-Regular.ttf");
        usernameField.setTypeface(tf_reg);
        passwordField.setTypeface(tf_reg);
        confirmPasswordField.setTypeface(tf_reg);

        Typeface tf_bold = Typeface.createFromAsset(getContext().getAssets(), "fonts/Lato-Bold.ttf");
        createAccountButton = (Button) v.findViewById(R.id.create_account);
        createAccountButton.setTypeface(tf_bold);

        usernameField.setText(username);
        passwordField.setText(password);

        if (config.isParseLoginEmailAsUsername()) {
            usernameField.setHint(R.string.com_parse_ui_email_input_hint);
            usernameField.setInputType(InputType.TYPE_TEXT_VARIATION_EMAIL_ADDRESS);
            if (emailField != null) {
                emailField.setVisibility(View.GONE);
            }
        }

        if (config.getParseSignupSubmitButtonText() != null) {
            createAccountButton.setText(config.getParseSignupSubmitButtonText());
        }
        createAccountButton.setOnClickListener(this);

        return v;
    }

    override public void onAttach(Activity activity) {
        base.onAttach(activity);
        if (activity is ParseOnLoginSuccessListener) {
            onLoginSuccessListener = (ParseOnLoginSuccessListener) activity;
        } else {
            throw new ArgumentException(
                    "Activity must implemement ParseOnLoginSuccessListener");
        }

        if (activity is ParseOnLoadingListener) {
            onLoadingListener = (ParseOnLoadingListener) activity;
        } else {
            throw new ArgumentException(
                    "Activity must implemement ParseOnLoadingListener");
        }
    }

    override public void onClick(View v) {

        ParseQuery<ParseObject> query = new ParseQuery<>("Group");
        query.whereEqualTo("objectId", getString(R.string.starter_group_id));
        query.fromLocalDatastore();
        query.findInBackground(new FindCallback<ParseObject>() {
            override public void done(List<ParseObject> groups, ParseException e) {
                // Find the starter group for all new Users
                if (e == null) for (ParseObject groupObject : groups) {
                    Log.w(GetType().ToString(), groupObject.getObjectId());

                    // Complete User registration
                    string username = usernameField.getText().ToString().ToLower().Replace(" ", "");
                    string password = passwordField.getText().ToString();
                    string passwordAgain = confirmPasswordField.getText().ToString();

                    string email = null;
                    if (config.isParseLoginEmailAsUsername()) {
                        email = usernameField.getText().ToString();
                    } else if (emailField != null) {
                        email = emailField.getText().ToString();
                    }

                    if (username.Length == 0) {
                        if (config.isParseLoginEmailAsUsername()) {
                            showToast(R.string.com_parse_ui_no_email_toast);
                        } else {
                            showToast(R.string.com_parse_ui_no_username_toast);
                        }
                    } else if (password.Length == 0) {
                        showToast(R.string.com_parse_ui_no_password_toast);
                    } else if (password.Length < minPasswordLength) {
                        showToast(getResources().getQuantityString(
                                R.plurals.com_parse_ui_password_too_short_toast,
                                minPasswordLength, minPasswordLength));
                    } else if (passwordAgain.Length == 0) {
                        showToast(R.string.com_parse_ui_reenter_password_toast);
                    } else if (!password.Equals(passwordAgain)) {
                        showToast(R.string.com_parse_ui_mismatch_confirm_password_toast);
                        confirmPasswordField.selectAll();
                        confirmPasswordField.requestFocus();
                    } else if (email != null && email.Length == 0) {
                        showToast(R.string.com_parse_ui_no_email_toast);
                        //    } else if (name != null && name.Length == 0) {
                        //      showToast(R.string.com_parse_ui_no_name_toast);
                    } else {
                        ParseUser user = new ParseUser();

                        // HashSet standard fields
                        user.setUsername(username);
                        user.setPassword(password);

                        // HashSet group fields
                        user.Add("currentGroup", groupObject);
                        string[] myGroups = {getString(R.string.starter_group_id)};
                        user.Add("myGroups", Array.asList(myGroups));

                        // Save the new User
                        user.saveInBackground();

                        loadingStart();
                        user.signUpInBackground(new SignUpCallback() {

                            override public void done(ParseException e) {
                                if (isActivityDestroyed()) {
                                    return;
                                }

                                if (e == null) {
                                    updateParseInstallation(ParseUser.getCurrentUser());

                                    loadingFinish();
                                    signupSuccess();
                                } else {
                                    loadingFinish();
                                    if (e != null) {
                                        debugLog(getString(R.string.com_parse_ui_login_warning_parse_signup_failed) +
                                                e.ToString());
                                        switch (e.getCode()) {
                                            case ParseException.INVALID_EMAIL_ADDRESS:
                                                showToast(R.string.com_parse_ui_invalid_email_toast);
                                                break;
                                            case ParseException.USERNAME_TAKEN:
                                                showToast(R.string.com_parse_ui_username_taken_toast);
                                                break;
                                            case ParseException.EMAIL_TAKEN:
                                                showToast(R.string.com_parse_ui_email_taken_toast);
                                                break;
                                            default:
                                                showToast(R.string.com_parse_ui_signup_failed_unknown_toast);
                                        }
                                    }
                                }
                            }
                        });
                    }

                }
            }
        });


    }

    public void updateParseInstallation(ParseUser user) {

        ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
        userQuery.whereEqualTo("objectId", ParseUser.getCurrentUser().getObjectId());
        userQuery.fromLocalDatastore();
        userQuery.findInBackground(new FindCallback<ParseUser>() {
            override public void done(List<ParseUser> users, ParseException e) {
                // Find the starter group for all new Users
                if (e == null) for (ParseObject userObject : users) {
                    string currentGroupObjectId = userObject.getParseObject("currentGroup").getObjectId();
                    Log.w(GetType().ToString(), currentGroupObjectId);

                    // Update Installation
                    ParseInstallation installation = ParseInstallation.getCurrentInstallation();
                    installation.Add("username", user.getUsername());
                    if (user["profilePicture"] != null) {
                        installation.Add("profilePicture", user["profilePicture"]);
                    }
                    installation.Add("groupId", currentGroupObjectId);
                    installation.Add("GCMSenderId", getString(R.string.gcm_sender_id));
                    installation.Add("userId", user.getObjectId());
                    installation.saveInBackground();

                }
            }
        });
    }

    override protected string getLogTag() {
        return LOG_TAG;
    }

    private void signupSuccess() {
        onLoginSuccessListener.onLoginSuccess();
    }

}
}
