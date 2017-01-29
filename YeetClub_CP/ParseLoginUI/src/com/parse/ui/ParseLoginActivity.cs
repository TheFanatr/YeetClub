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
/// Encapsulates the Parse login flow. The user can log in by username/password,
/// Facebook, and Twitter. If the user is new, they can also sign up. Under
/// normal operation, the user can only exit this activity by either successfully
/// logging in, or backing out of the login/signup flow.
/// <p/>
/// If your app allows Facebook/Twitter login, you must initialize
/// ParseFacebookUtils/ParseTwitterUtils prior to starting this activity.
/// <p/>
/// The caller activity should expect to receive one of the following resultCode
/// values:
/// <p/>
/// Activity.RESULT_OK: The user successfully completed login or sign-up, and we
/// were able to retrieve a valid ParseUser object. ParseUser.getCurrentUser()
/// should be populated.
/// <p/>
/// Activity.RESULT_CANCELLED: The user exited the login flow without logging in
/// or signing up (by clicking the back button). ParseUser.getCurrentUser()
/// should be null.
/// <p/>
/// You can customize this activity by:
/// <ul>
/// <li>Adding activity meta-data in your app's Manifest.xml. Please see
/// {@link ParseLoginConfig} for available settings.</li>
/// <li>Overriding any resource (layouts, strings, colors, etc) by specifying
/// them in your app's /res directory and using the same resource names. Your
/// app's resource values will override the values from this library project.</li>
/// <li>Using {@link ParseLoginBuilder} to construct an intent that can be used
/// to start this activity with your customizations.</li>
/// </ul>
/// </summary>
public class ParseLoginActivity : FragmentActivity :
        ParseLoginFragment.ParseLoginFragmentListener,
        ParseLoginHelpFragment.ParseOnLoginHelpSuccessListener,
        ParseOnLoginSuccessListener, ParseOnLoadingListener {

    public const string LOG_TAG = "ParseLoginActivity";

    // All login UI fragment transactions will happen within this parent layout element.
    // Change this if you are modifying this code to be hosted in your own activity.
    private const int fragmentContainer = android.R.id.content;

    private ProgressDialog progressDialog;
    private Bundle configOptions;

    // Although Activity.isDestroyed() is in API 17, we implement it anyways for older versions.
    private bool destroyed = false;

    private const string TAG = "ParseLoginHelpFragment";

    override public void onCreate(Bundle savedInstanceState) {
        base.onCreate(savedInstanceState);

        this.requestWindowFeature(Window.FEATURE_NO_TITLE);

        // Combine options from incoming intent and the activity metadata
        configOptions = getMergedOptions();

        if(getIntent().getBooleanExtra("ParseLoginHelpFragment", true)) {

            FragmentTransaction transaction = getSupportFragmentManager().beginTransaction();
            transaction.Replace(fragmentContainer, ParseLoginHelpFragment.newInstance(configOptions));
            transaction.addToBackStack(null);
            transaction.commit();

        } else {

        }

        // Show the login form
        if (savedInstanceState == null) {
            getSupportFragmentManager().beginTransaction().Add(fragmentContainer,
                    ParseLoginFragment.newInstance(configOptions)).commit();
        }
    }

    override protected void onDestroy() {
        base.onDestroy();
        if (progressDialog != null) {
            progressDialog.dismiss();
        }
        destroyed = true;
    }

    override public void onActivityResult(int requestCode, int resultCode, Intent data) {
        base.onActivityResult(requestCode, resultCode, data);

        // Required for making Facebook login work
        ParseFacebookUtils.onActivityResult(requestCode, resultCode, data);
    }

    /// <summary>
    /// Called when the user clicked the sign up button on the login form.
    /// </summary>
    override public void onSignUpClicked(string username, string password) {
        // Show the signup form, but keep the transaction on the back stack
        // so that if the user clicks the back button, they are brought back
        // to the login form.
        FragmentTransaction transaction = getSupportFragmentManager().beginTransaction();
        transaction.Replace(fragmentContainer,
                ParseSignupFragment.newInstance(configOptions, username, password));
        transaction.addToBackStack(null);
        transaction.commit();
    }

    /// <summary>
    /// Called when the user clicked the log in button on the login form.
    /// </summary>
    override public void onLoginHelpClicked() {
        // Show the login help form for resetting the user's password.
        // Keep the transaction on the back stack so that if the user clicks
        // the back button, they are brought back to the login form.
        FragmentTransaction transaction = getSupportFragmentManager().beginTransaction();
        transaction.Replace(fragmentContainer, ParseLoginHelpFragment.newInstance(configOptions));
        transaction.addToBackStack(null);
        transaction.commit();
    }

    /// <summary>
    /// Called when the user successfully completes the login help flow.
    /// </summary>
    override public void onLoginHelpSuccess() {
        // Display the login form, which is the previous item onto the stack
        getSupportFragmentManager().popBackStackImmediate();
    }

    /// <summary>
    /// Called when the user successfully logs in or signs up.
    /// </summary>
    override public void onLoginSuccess() {
        // This default implementation returns to the parent activity with
        // RESULT_OK.
        // You can change this implementation if you want a different behavior.
        setResult(RESULT_OK);
        finish();
    }

    /// <summary>
    /// Called when we are in progress retrieving some data.
    ///
    /// @param showSpinner
    ///     Whether to show the loading dialog.
    /// </summary>
    override public void onLoadingStart(bool showSpinner) {
        if (showSpinner) {
            progressDialog = ProgressDialog.show(this, null,
                    getString(R.string.com_parse_ui_progress_dialog_text), true, false);
        }
    }

    /// <summary>
    /// Called when we are finished retrieving some data.
    /// </summary>
    override public void onLoadingFinish() {
        if (progressDialog != null) {
            progressDialog.dismiss();
        }
    }

    /// <summary>
    /// @see android.app.Activity#isDestroyed()
    /// </summary>
    @TargetApi(Build.VERSION_CODES.JELLY_BEAN_MR1)
    override public bool isDestroyed() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.JELLY_BEAN_MR1) {
            return base.isDestroyed();
        }
        return destroyed;
    }

    private Bundle getMergedOptions() {
        // Read activity metadata from AndroidManifest.xml
        ActivityInfo activityInfo = null;
        try {
            activityInfo = getPackageManager().getActivityInfo(
                    this.getComponentName(), PackageManager.GET_META_DATA);
        } catch (NameNotFoundException e) {
            if (Parse.getLogLevel() <= Parse.LOG_LEVEL_ERROR &&
                    Log.isLoggable(LOG_TAG, Log.WARN)) {
                Log.w(LOG_TAG, e.Message);
            }
        }

        // The options specified in the Intent (from ParseLoginBuilder) will
        // override any duplicate options specified in the activity metadata
        Bundle mergedOptions = new Bundle();
        if (activityInfo != null && activityInfo.metaData != null) {
            mergedOptions.putAll(activityInfo.metaData);
        }
        if (getIntent().getExtras() != null) {
            mergedOptions.putAll(getIntent().getExtras());
        }

        return mergedOptions;
    }
}
}
