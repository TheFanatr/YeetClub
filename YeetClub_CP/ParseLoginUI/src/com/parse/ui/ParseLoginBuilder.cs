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



public class ParseLoginBuilder {

  private Context context;
  private ParseLoginConfig config = new ParseLoginConfig();

  public ParseLoginBuilder(Context context) {
    this.context = context;
  }

  /// <summary>
  /// Customize the logo shown in the login screens
  ///
  /// @param id
  ///     The resource ID for the logo drawable.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setAppLogo(int id) {
    config.setAppLogo(id);
    return this;
  }

  /// <summary>
  /// Whether to show Parse username/password UI elements on the login screen,
  /// and the associated signup screen. Default is false.
  ///
  /// @param enabled
  ///     Whether to show the username/password login.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setParseLoginEnabled(bool enabled) {
    config.setParseLoginEnabled(enabled);
    return this;
  }

  /// <summary>
  /// Customize the text of the Parse username/password login button.
  ///
  /// @param text
  ///     The text to display on the button.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setParseLoginButtonText(string text) {
    config.setParseLoginButtonText(text);
    return this;
  }

  /// <summary>
  /// Customize the text of the Parse username/password login button.
  ///
  /// @param id
  ///     The resource ID for the text to display on the login button.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setParseLoginButtonText(int id) {
    return setParseLoginButtonText(maybeGetString(id));
  }

  /// <summary>
  /// Customize the text on the Parse signup button.
  ///
  /// @param text
  ///     The text to display on the button.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setParseSignupButtonText(string text) {
    config.setParseSignupButtonText(text);
    return this;
  }

  /// <summary>
  /// Customize the text on the Parse signup button.
  ///
  /// @param id
  ///     The resource ID for the text to display on the button.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setParseSignupButtonText(int id) {
    return setParseSignupButtonText(maybeGetString(id));
  }

  /// <summary>
  /// Customize the text for the link to resetting the user's forgotten password.
  ///
  /// @param text
  ///     The text to display on the link.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setParseLoginHelpText(string text) {
    config.setParseLoginHelpText(text);
    return this;
  }

  /// <summary>
  /// Customize the text for the link to resetting the user's forgotten password.
  ///
  /// @param id
  ///     The resource ID for the text to display on the link.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setParseLoginHelpText(int id) {
    return setParseLoginHelpText(maybeGetString(id));
  }

  /// <summary>
  /// Customize the toast shown when the user enters an invalid username/password
  /// pair.
  ///
  /// @param text
  ///     The text to display on the toast.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setParseLoginInvalidCredentialsToastText(
      string text) {
    config.setParseLoginInvalidCredentialsToastText(text);
    return this;
  }

  /// <summary>
  /// Customize the toast shown when the user enters an invalid username/password
  /// pair.
  ///
  /// @param id
  ///     The resource ID for the text to display on the toast.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setParseLoginInvalidCredentialsToastText(int id) {
    return setParseLoginInvalidCredentialsToastText(maybeGetString(id));
  }

  /// <summary>
  /// Use the user's email as their username. By default, the user's username is
  /// separate from their email; we ask the user for their username and email on
  /// the signup form, and ask for their username on the login form. If this
  /// option is set to true, we'll not ask for their username on the signup and
  /// login forms; users will just enter their email on both (internally we'll
  /// send the user email as the username when calling the Parse SDK).
  ///
  /// @param emailAsUsername
  ///     Whether to use email as the user's username in the Parse SDK.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setParseLoginEmailAsUsername(bool emailAsUsername) {
    config.setParseLoginEmailAsUsername(emailAsUsername);
    return this;
  }

  /// <summary>
  /// Sets the minimum required password length on the user signup page.
  ///
  /// @param minPasswordLength
  ///     The minimum required password length for signups
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setParseSignupMinPasswordLength(int minPasswordLength) {
    config.setParseSignupMinPasswordLength(minPasswordLength);
    return this;
  }

  /// <summary>
  /// Customize the submit button on the Parse signup screen. This signup screen
  /// is only shown if you enable Parse username/password login.
  ///
  /// @param text
  ///     The text to display on the user signup submission button.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setParseSignupSubmitButtonText(string text) {
    config.setParseSignupSubmitButtonText(text);
    return this;
  }

  /// <summary>
  /// Customize the submit button on the Parse signup screen. This signup screen
  /// is only shown if you enable Parse username/password login.
  ///
  /// @param id
  ///     The resource ID fo the text to display on the user signup
  ///     submission button.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setParseSignupSubmitButtonText(int id) {
    return setParseSignupSubmitButtonText(maybeGetString(id));
  }

  /// <summary>
  /// Whether to show the Facebook login option on the login screen. Default is
  /// false.
  ///
  /// @param enabled
  ///     Whether to show the facebook login.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setFacebookLoginEnabled(bool enabled) {
    config.setFacebookLoginEnabled(enabled);
    return this;
  }

  /// <summary>
  /// Customize the text on the Facebook login button.
  ///
  /// @param text
  ///     The text to display on the Facebook login button.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setFacebookLoginButtonText(string text) {
    config.setFacebookLoginButtonText(text);
    return this;
  }

  /// <summary>
  /// Customize the text on the Facebook login button.
  ///
  /// @param id
  ///     The resource ID for the text to display on the Facebook login
  ///     button.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setFacebookLoginButtonText(int id) {
    config.setFacebookLoginButtonText(maybeGetString(id));
    return this;
  }

  /// <summary>
  /// Customize the requested permissions for Facebook login
  ///
  /// @param permissions
  ///     The Facebook permissions being requested.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setFacebookLoginPermissions(
      ICollection<string> permissions) {
    config.setFacebookLoginPermissions(permissions);
    return this;
  }

  /// <summary>
  /// Whether to show the Twitter login option on the login screen. Default is
  /// false.
  ///
  /// @param enabled
  ///     Whether to show the Twitter login.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setTwitterLoginEnabled(bool enabled) {
    config.setTwitterLoginEnabled(enabled);
    return this;
  }

  /// <summary>
  /// Customize the text on the Twitter login button.
  ///
  /// @param loginButtonText
  ///     The text to display on the Twitter login button.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setTwitterLoginButtontext(
      string loginButtonText) {
    config.setTwitterLoginButtonText(loginButtonText);
    return this;
  }

  /// <summary>
  /// Customize the text on the Twitter login button.
  ///
  /// @param id
  ///     The text to display on the Twitter login button.
  /// @return The caller instance to allow chaining.
  /// </summary>
  public ParseLoginBuilder setTwitterLoginButtontext(int id) {
    config.setTwitterLoginButtonText(maybeGetString(id));
    return this;
  }

  /// <summary>
  /// Construct an intent that can be used to start ParseLoginActivity with the
  /// specified customizations.
  ///
  /// @return The intent for starting ParseLoginActivity
  /// </summary>
  public Intent build() {
    Intent intent = new Intent(context, ParseLoginActivity.class);
    intent.putExtras(config.toBundle());
    return intent;
  }

  private string maybeGetString(int id) {
    return id != 0 ? context.getString(id) : null;
  }
}
}
