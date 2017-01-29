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
/// Fragment for the login help screen for resetting the user's password.
/// </summary>
public class ParseLoginHelpFragment : ParseLoginFragmentBase : OnClickListener {

  public interface ParseOnLoginHelpSuccessListener {
    public void onLoginHelpSuccess();
  }

  private TextView instructionsTextView;
  private EditText emailField;
  private Button submitButton;
  private bool emailSent = false;
  private ParseOnLoginHelpSuccessListener onLoginHelpSuccessListener;

  private ParseLoginConfig config;

  private string LOG_TAG = "ParseLoginHelpFragment";

  public static ParseLoginHelpFragment newInstance(Bundle configOptions) {
    ParseLoginHelpFragment loginHelpFragment = new ParseLoginHelpFragment();
    loginHelpFragment.setArguments(configOptions);
    return loginHelpFragment;
  }

  override public View onCreateView(LayoutInflater inflater, ViewGroup parent,
                           Bundle savedInstanceState) {

    config = ParseLoginConfig.fromBundle(getArguments(), getActivity());

    View v = inflater.inflate(R.layout.com_parse_ui_parse_login_help_fragment,
        parent, false);
    instructionsTextView = (TextView) v
        .findViewById(R.id.login_help_instructions);
    emailField = (EditText) v.findViewById(R.id.login_help_email_input);
    submitButton = (Button) v.findViewById(R.id.login_help_submit);

    submitButton.setOnClickListener(this);
    return v;
  }

  override public void onAttach(Activity activity) {
    base.onAttach(activity);

    if (activity is ParseOnLoadingListener) {
      onLoadingListener = (ParseOnLoadingListener) activity;
    } else {
      throw new ArgumentException(
          "Activity must implemement ParseOnLoadingListener");
    }

    if (activity is ParseOnLoginHelpSuccessListener) {
      onLoginHelpSuccessListener = (ParseOnLoginHelpSuccessListener) activity;
    } else {
      throw new ArgumentException(
          "Activity must implemement ParseOnLoginHelpSuccessListener");
    }
  }

  override public void onClick(View v) {
    if (!emailSent) {
      string email = emailField.getText().ToString();
      if (email.Length == 0) {
        showToast(R.string.com_parse_ui_no_email_toast);
      } else {
        loadingStart();
        ParseUser.requestPasswordResetInBackground(email,
            new RequestPasswordResetCallback() {
              override public void done(ParseException e) {
                if (isActivityDestroyed()) {
                  return;
                }

                loadingFinish();
                if (e == null) {
                  instructionsTextView
                      .setText(R.string.com_parse_ui_login_help_email_sent);
                  emailField.setVisibility(View.INVISIBLE);
                  submitButton
                      .setText(R.string.com_parse_ui_login_help_login_again_button_label);
                  emailSent = true;
                } else {
                  debugLog(getString(R.string.com_parse_ui_login_warning_password_reset_failed) +
                      e.ToString());
                  if (e.getCode() == ParseException.INVALID_EMAIL_ADDRESS ||
                      e.getCode() == ParseException.EMAIL_NOT_FOUND) {
                    showToast(R.string.com_parse_ui_invalid_email_toast);
                  } else {
                    showToast(R.string.com_parse_ui_login_help_submit_failed_unknown);
                  }
                }
              }
            });
      }
    } else {
      onLoginHelpSuccessListener.onLoginHelpSuccess();
    }
  }

  override protected string getLogTag() {
    return LOG_TAG;
  }
}
}
