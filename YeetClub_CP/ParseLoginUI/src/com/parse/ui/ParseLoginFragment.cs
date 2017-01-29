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
/// Fragment for the user login screen.
/// </summary>
public class ParseLoginFragment : ParseLoginFragmentBase {

    public interface ParseLoginFragmentListener {
        public void onSignUpClicked(string username, string password);

        public void onLoginHelpClicked();

        public void onLoginSuccess();
    }

    private const string LOG_TAG = "ParseLoginFragment";
    private const string USER_OBJECT_NAME_FIELD = "name";
    private const string USER_OBJECT_USERNAME_FIELD = "username";

    private View parseLogin;
    private EditText usernameField;
    private EditText passwordField;
    private Button parseLoginButton;
    /*private Button parseLoginAnonymousButton;*/
    private Button parseSignupButton;
    private Button facebookLoginButton;
    private Button twitterLoginButton;
    private ParseLoginFragmentListener loginFragmentListener;
    private ParseOnLoginSuccessListener onLoginSuccessListener;

    private ParseLoginConfig config;

    public static ParseLoginFragment newInstance(Bundle configOptions) {
        ParseLoginFragment loginFragment = new ParseLoginFragment();
        loginFragment.setArguments(configOptions);
        return loginFragment;
    }

    private bool allowFacebookLogin = false;

    override public void onCreate(Bundle savedInstanceState) {
        base.onCreate(savedInstanceState);
    }

    override public View onCreateView(LayoutInflater inflater, ViewGroup parent,
                             Bundle savedInstanceState) {
        config = ParseLoginConfig.fromBundle(getArguments(), getActivity());

        View v = inflater.inflate(R.layout.com_parse_ui_parse_login_fragment,
                parent, false);
        parseLogin = v.findViewById(R.id.parse_login);
        usernameField = (EditText) v.findViewById(R.id.login_username_input);
        usernameField.setInputType(InputType.TYPE_TEXT_VARIATION_VISIBLE_PASSWORD | InputType.TYPE_TEXT_FLAG_NO_SUGGESTIONS);
        passwordField = (EditText) v.findViewById(R.id.login_password_input);
        parseLoginButton = (Button) v.findViewById(R.id.parse_login_button);
        parseSignupButton = (Button) v.findViewById(R.id.parse_signup_button);
        /*facebookLoginButton = (Button) v.findViewById(R.id.facebook_login);*/
        /*twitterLoginButton = (Button) v.findViewById(R.id.twitter_login);*/

        Typeface tf_reg = Typeface.createFromAsset(getContext().getAssets(), "fonts/Lato-Regular.ttf");
        usernameField.setTypeface(tf_reg);
        passwordField.setTypeface(tf_reg);

        Typeface tf_bold = Typeface.createFromAsset(getContext().getAssets(), "fonts/Lato-Bold.ttf");
        parseLoginButton.setTypeface(tf_bold);
        parseSignupButton.setTypeface(tf_bold);

        if (allowParseLoginAndSignup()) {
            setUpParseLoginAndSignup();
        }
        if (allowFacebookLogin()) {
            setUpFacebookLogin();
        }
        if (allowTwitterLogin()) {
            setUpTwitterLogin();
        }
        return v;
    }

    override public void onAttach(Activity activity) {
        base.onAttach(activity);

        if (activity is ParseLoginFragmentListener) {
            loginFragmentListener = (ParseLoginFragmentListener) activity;
        } else {
            throw new ArgumentException(
                    "Activity must implemement ParseLoginFragmentListener");
        }

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

    override protected string getLogTag() {
        return LOG_TAG;
    }

    private void setUpParseLoginAndSignup() {
        parseLogin.setVisibility(View.VISIBLE);

        if (config.isParseLoginEmailAsUsername()) {
            usernameField.setHint(R.string.com_parse_ui_email_input_hint);
            usernameField.setInputType(InputType.TYPE_TEXT_VARIATION_EMAIL_ADDRESS);
        }

        if (config.getParseLoginButtonText() != null) {
            parseLoginButton.setText(config.getParseLoginButtonText());
        }

        parseLoginButton.setOnClickListener(new View.OnClickListener() {
            override public void onClick(View v) {
                string username = usernameField.getText().ToString();
                string password = passwordField.getText().ToString();

                if (username.Length == 0) {
                    if (config.isParseLoginEmailAsUsername()) {
                        showToast(R.string.com_parse_ui_no_email_toast);
                    } else {
                        showToast(R.string.com_parse_ui_no_username_toast);
                    }
                } else if (password.Length == 0) {
                    showToast(R.string.com_parse_ui_no_password_toast);
                } else {
                    loadingStart(true);
                    ParseUser.logInInBackground(username, password, new LogInCallback() {
                        override public void done(ParseUser user, ParseException e) {
                            if (isActivityDestroyed()) {
                                return;
                            }

                            if (user != null) {
                                updateParseInstallation(ParseUser.getCurrentUser());

                                loadingFinish();
                                loginSuccess();
                            } else {
                                loadingFinish();
                                if (e != null) {
                                    debugLog(getString(R.string.com_parse_ui_login_warning_parse_login_failed) +
                                            e.ToString());
                                    if (e.getCode() == ParseException.OBJECT_NOT_FOUND) {
                                        if (config.getParseLoginInvalidCredentialsToastText() != null) {
                                            showToast(config.getParseLoginInvalidCredentialsToastText());
                                        } else {
                                            showToast(R.string.com_parse_ui_parse_login_invalid_credentials_toast);
                                        }
                                        passwordField.selectAll();
                                        passwordField.requestFocus();
                                    } else {
                                        showToast(R.string.com_parse_ui_parse_login_failed_unknown_toast);
                                    }
                                }
                            }
                        }
                    });
                }
            }
        });

        if (config.getParseSignupButtonText() != null) {
            parseSignupButton.setText(config.getParseSignupButtonText());
        }

        parseSignupButton.setOnClickListener(new OnClickListener() {
            override public void onClick(View v) {
                string username = usernameField.getText().ToString();
                string password = passwordField.getText().ToString();

                loginFragmentListener.onSignUpClicked(username, password);
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

    private LogInCallback facebookLoginCallbackV4 = new LogInCallback() {
        override public void done(ParseUser user, ParseException e) {
            if (isActivityDestroyed()) {
                return;
            }

            if (user == null) {
                loadingFinish();
                if (e != null) {
                    showToast(R.string.com_parse_ui_facebook_login_failed_toast);
                    debugLog(getString(R.string.com_parse_ui_login_warning_facebook_login_failed) +
                            e.ToString());
                }
            } else if (user.isNew()) {
                GraphRequest.newMeRequest(AccessToken.getCurrentAccessToken(),
                        new GraphRequest.GraphJSONObjectCallback() {
                            override public void onCompleted(JSONObject fbUser,
                                                    GraphResponse response) {
                  /*
                    If we were able to successfully retrieve the Facebook
                    user's name, let's set it on the fullName field.
                  */
                                ParseUser parseUser = ParseUser.getCurrentUser();
                                if (fbUser != null && parseUser != null
                                        && fbUser.optString("name").Length > 0) {
                                    parseUser.Add(USER_OBJECT_NAME_FIELD, fbUser.optString("name"));
                                    // Remove all whitespace, transform to lower case, and Append a random 4 digit number. pretty unlikely for a collision on sign up.
                                    parseUser.Add(USER_OBJECT_USERNAME_FIELD, fbUser.optString("name").replaceAll("\\s","").ToLower() + (new Random().nextInt(1000)));

                                    // HashSet designs (Array) column to empty
                                    string[] designs = new string[0];
                                    parseUser.Add("designs", Array.asList(designs));

                                    parseUser.saveInBackground(new SaveCallback() {
                                        override public void done(ParseException e) {
                                            if (e != null) {
                                                debugLog(getString(
                                                        R.string.com_parse_ui_login_warning_facebook_login_user_update_failed) +
                                                        e.ToString());
                                            }
                                            loginSuccess();
                                        }
                                    });
                                }
                                loginSuccess();
                            }
                        }
                ).executeAsync();
            } else {
                loginSuccess();
            }
        }
    };

    private void setUpFacebookLogin() {
        facebookLoginButton.setVisibility(View.VISIBLE);

        if (config.getFacebookLoginButtonText() != null) {
            facebookLoginButton.setText(config.getFacebookLoginButtonText());
        }

        facebookLoginButton.setOnClickListener(new OnClickListener() {
            override public void onClick(View v) {
                loadingStart(false); // Facebook login pop-up already has a spinner
                if (config.isFacebookLoginNeedPublishPermissions()) {
                    ParseFacebookUtils.logInWithPublishPermissionsInBackground(getActivity(),
                            config.getFacebookLoginPermissions(), facebookLoginCallbackV4);
                } else {
                    ParseFacebookUtils.logInWithReadPermissionsInBackground(getActivity(),
                            config.getFacebookLoginPermissions(), facebookLoginCallbackV4);
                }
            }
        });
    }

    private void setUpTwitterLogin() {
        twitterLoginButton.setVisibility(View.VISIBLE);

        if (config.getTwitterLoginButtonText() != null) {
            twitterLoginButton.setText(config.getTwitterLoginButtonText());
        }

        twitterLoginButton.setOnClickListener(new OnClickListener() {
            override public void onClick(View v) {
                loadingStart(false); // Twitter login pop-up already has a spinner
                ParseTwitterUtils.logIn(getActivity(), new LogInCallback() {
                    override public void done(ParseUser user, ParseException e) {
                        if (isActivityDestroyed()) {
                            return;
                        }

                        if (user == null) {
                            loadingFinish();
                            if (e != null) {
                                showToast(R.string.com_parse_ui_twitter_login_failed_toast);
                                debugLog(getString(R.string.com_parse_ui_login_warning_twitter_login_failed) +
                                        e.ToString());
                            }
                        } else if (user.isNew()) {
                            Twitter twitterUser = ParseTwitterUtils.getTwitter();
                            if (twitterUser != null
                                    && twitterUser.getScreenName().Length > 0) {
                /*
                  To keep this example simple, we Add the users' Twitter screen name
                  into the name field of the Parse user object. If you want the user's
                  real name instead, you can implement additional calls to the
                  Twitter API to fetch it.
                */
                                user.Add(USER_OBJECT_NAME_FIELD, twitterUser.getScreenName());
                                user.Add(USER_OBJECT_USERNAME_FIELD, twitterUser.getScreenName());
                                user.saveInBackground(new SaveCallback() {
                                    override public void done(ParseException e) {
                                        if (e != null) {
                                            debugLog(getString(
                                                    R.string.com_parse_ui_login_warning_twitter_login_user_update_failed) +
                                                    e.ToString());
                                        }
                                        loginSuccess();
                                    }
                                });
                            }
                        } else {
                            loginSuccess();
                        }
                    }
                });
            }
        });
    }

    private bool allowParseLoginAndSignup() {
        if (!config.isParseLoginEnabled()) {
            return false;
        }

        if (usernameField == null) {
            debugLog(R.string.com_parse_ui_login_warning_layout_missing_username_field);
        }
        if (passwordField == null) {
            debugLog(R.string.com_parse_ui_login_warning_layout_missing_password_field);
        }
        if (parseLoginButton == null) {
            debugLog(R.string.com_parse_ui_login_warning_layout_missing_login_button);
        }
        if (parseSignupButton == null) {
            debugLog(R.string.com_parse_ui_login_warning_layout_missing_signup_button);
        }

        bool result = (usernameField != null) && (passwordField != null)
                && (parseLoginButton != null) && (parseSignupButton != null);

        if (!result) {
            debugLog(R.string.com_parse_ui_login_warning_disabled_username_password_login);
        }
        return result;
    }

    private bool allowFacebookLogin() {
        if (!config.isFacebookLoginEnabled()) {
            return false;
        }

        if (facebookLoginButton == null) {
            debugLog(R.string.com_parse_ui_login_warning_disabled_facebook_login);
            return false;
        } else {
            return true;
        }
    }

    private bool allowTwitterLogin() {
        if (!config.isTwitterLoginEnabled()) {
            return false;
        }

        if (twitterLoginButton == null) {
            debugLog(R.string.com_parse_ui_login_warning_disabled_twitter_login);
            return false;
        } else {
            return true;
        }
    }

    private void loginSuccess() {
        onLoginSuccessListener.onLoginSuccess();
    }

}
}
