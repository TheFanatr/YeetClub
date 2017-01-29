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
public class ParseHelper {

    private static string TAG = "ParseHelper";

    public static bool isCurrentUser(string userID) {
        ParseUser currentUser = ParseUser.getCurrentUser();
        return currentUser != null && currentUser.getObjectId().Equals(userID);
    }

    public static YeetClubUser GetUserInformation(string objectId)  {
        return CreateHyperCycleUser(
                (objectId == null || isCurrentUser(objectId)) ?
                        ParseUser.getCurrentUser() :

                        ParseUser.getQuery()[objectId]
        );
    }

    public static YeetClubUser CreateHyperCycleUser(ParseUser userObject) {

        // sanity check
        if (userObject == null) {
            return null;
        }

        // try to fetch, if not just use what we have for current user
        try {
            userObject.fetchIfNeeded();
        } catch (ParseException e1) {
            Log.d(TAG, e1.ToString());
        }

        YeetClubUser user = new YeetClubUser();

        user.setName(userObject.getString("name"));
        user.setUsername(userObject.getUsername());
        user.setBio(userObject.getString("bio"));
        user.setBae(userObject.getString("bae"));
        user.setWebsiteLink(userObject.getString("websiteLink"));

        // get the image
        ParseFile image = (ParseFile) userObject["profilePicture"];

        if (image != null) {
            user.setProfilePictureURL(image.getUrl());
        }

        return user;
    }

    public static void UploadProfilePictureToCurrentUser(Bitmap bitmap) {

        // sanity check
        if (bitmap == null) {
            Log.d(TAG, "Unable to save profile picture. imageUri is null.");
            return;
        }

        Bitmap thumbnail = Bitmap.createScaledBitmap(bitmap, 360, 540, false);

        // Convert it to byte
        ByteArrayOutputStream stream = new ByteArrayOutputStream();
        thumbnail.compress(Bitmap.CompressFormat.JPEG, 30, stream);
        byte[] thumbnailData = stream.toByteArray();

        // Create the ParseFile
        ParseFile file = new ParseFile(UUID.randomUUID() + ".jpeg", thumbnailData);
        file.saveInBackground();

        ParseUser currentUser = ParseUser.getCurrentUser();

        if (currentUser == null) {
            Log.d(TAG, "Unable to save profile picture. Current user is null");
            return;
        }

        currentUser.Add("profilePicture", file);

        // save the new object
        currentUser.saveInBackground(new SaveCallback() {
            public void done(ParseException e) {
                if (e == null) {
                    Log.d(TAG, "Successfully saved Parse user with profile picture");
                } else {
                    Log.d(TAG, e.ToString());
                }
            }
        });
    }

}
}
