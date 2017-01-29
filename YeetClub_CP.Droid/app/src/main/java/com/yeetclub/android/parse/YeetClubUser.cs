using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.parse
{

/// <summary>
/// Created by @santafebound on 2015-11-07.
/// </summary>
public class YeetClubUser {

    private string name;
    private string username;
    private string bio;
    private string bae;
    private string websiteLink;
    private string profilePictureURL;

    public string getName() {
        return name;
    }

    public void setName(string name) {
        this.name = name;
    }

    public string getUsername() {
        return username;
    }

    public void setUsername(string username) {
        this.username = username;
    }

    public string getBio() {
        return bio;
    }

    public void setBio(string bio) {
        this.bio = this.bio;
    }

    public string getBae() {
        return bae;
    }

    public void setBae(string bae) {
        this.bae = this.bae;
    }

    public string getWebsiteLink() {
        return websiteLink;
    }

    public void setWebsiteLink(string websiteLink) {
        this.websiteLink = this.websiteLink;
    }

    public string getProfilePictureURL() {
        return profilePictureURL;
    }

    public void setProfilePictureURL(string profilePictureURL) {
        this.profilePictureURL = profilePictureURL;
    }
}
}
