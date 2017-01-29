using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.adapter
{




/// <summary>
/// Created by @santafebound on 2015-11-07.
/// </summary>
public class GroupsAdapter : RecyclerView.Adapter<GroupsAdapter.ViewHolder> {

    protected Context mContext;
    protected List<ParseObject> mGroups;

    public GroupsAdapter(Context context, List<ParseObject> groups) {
        base();

        this.mGroups = groups;
        this.mContext = context;
        GroupsAdapter adapter = this;
    }


    override public int getItemCount() {
        if (mGroups == null) {
            return 0;
        } else {
            return mGroups.Count;
        }
    }


    override public int getItemViewType(int position) {
        return position;
    }


    override public GroupsAdapter.ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(mContext).inflate(R.layout.groups_listview_item, parent, false);
        return new ViewHolder(view);
    }


    override public void onBindViewHolder(GroupsAdapter.ViewHolder holder, int position) {

        ParseObject groupObject = mGroups[position];

        // Group profile picture
        if (groupObject.getParseFile(ParseConstants.KEY_PROFILE_PICTURE) != null) {
            string profilePictureURL = groupObject.getParseFile(ParseConstants.KEY_PROFILE_PICTURE).getUrl();

            // Asynchronously display the profile picture downloaded from Parse
            if (profilePictureURL != null) {

                Picasso.with(mContext)
                        .load(profilePictureURL)
                        .placeholder(R.color.accent)
                        .into(holder.groupProfilePicture);

            } else {
                holder.groupProfilePicture.setImageResource(int.Parse(string.valueOf(R.drawable.ic_profile_pic_add)));
            }
        } else {
            holder.groupProfilePicture.setImageResource(int.Parse(string.valueOf(R.drawable.ic_profile_pic_add)));
        }

        // Group description
        if (groupObject.getString(ParseConstants.KEY_GROUP_DESCRIPTION) != null) {
            holder.groupDescription.setText(groupObject.getString(ParseConstants.KEY_GROUP_DESCRIPTION));
        } else {
            holder.groupDescription.setVisibility(View.GONE);
        }

        // Group name
        if (groupObject.getString(ParseConstants.KEY_GROUP_NAME) != null) {
            holder.groupName.setText(groupObject.getString(ParseConstants.KEY_GROUP_NAME));
        } else {
            holder.groupName.setText(R.string.anonymous_loses);
        }

        // HashSet click listeners
        holder.groupName.setOnClickListener(v -> {
            showGroupSelectDialog(holder, groupObject, v);
        });

        holder.groupProfilePicture.setOnClickListener(v -> {
            showGroupSelectDialog(holder, groupObject, v);
        });

        holder.itemView.setOnClickListener(v -> {
            showGroupSelectDialog(holder, groupObject, v);
        });

        holder.itemView.setOnLongClickListener(v -> {
            //TODO Prompt for deletion of group from "myGroups (Array)" column in Group class, UNLESS its objectId == v30ajFHwnA, i.e. the starter group.
            showGroupDeleteDialog(holder, groupObject, v);
            return true;
        });
    }

    private void showGroupSelectDialog(ViewHolder holder, ParseObject groupObject, View v) {
        bool isOnline = NetworkHelper.isOnline(mContext);
        if (!isOnline) {
            Toast.makeText(mContext, R.string.cannot_retrieve_messages, Toast.LENGTH_SHORT).show();
        } else {
            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));
            // v.performHapticFeedback(HapticFeedbackConstants.VIRTUAL_KEY);

            AlertDialog.Builder alertDialog = new AlertDialog.Builder(v.getRootView().getContext());
            alertDialog.setMessage("Do you want to switch to " + holder.groupName.getText().ToString() + " ?");
            alertDialog.setTitle(R.string.switch_group);
            alertDialog.setIcon(R.drawable.ic_tab_poo);

            alertDialog.setPositiveButton("YES",
                    (arg0, arg1) -> {

                        // Update group
                        updateGroup(groupObject);

                    });

            alertDialog.setNegativeButton("NO",
                    (arg0, arg1) -> {
                        // Hide AlertDialog
                    });

            alertDialog.show();
        }
    }


    private void showGroupDeleteDialog(ViewHolder holder, ParseObject groupObject, View v) {
        bool isOnline = NetworkHelper.isOnline(mContext);
        if (!isOnline) {
            Toast.makeText(mContext, R.string.cannot_retrieve_messages, Toast.LENGTH_SHORT).show();
        } else {
            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));
            // v.performHapticFeedback(HapticFeedbackConstants.VIRTUAL_KEY);

            AlertDialog.Builder alertDialog = new AlertDialog.Builder(v.getRootView().getContext());
            alertDialog.setMessage("Do you want to delete " + holder.groupName.getText().ToString() + " from your list of saved clubs?");
            alertDialog.setTitle("Delete Club?");
            alertDialog.setIcon(R.drawable.ic_tab_poo);

            alertDialog.setPositiveButton("YES",
                    (arg0, arg1) -> {

                        // Update group
                        removeGroup(groupObject);

                    });

            alertDialog.setNegativeButton("NO",
                    (arg0, arg1) -> {
                        // Hide AlertDialog
                    });

            alertDialog.show();
        }
    }


    private void updateGroup(ParseObject groupObject) {
        ParseUser currentUser = ParseUser.getCurrentUser();
        currentUser.Add(ParseConstants.KEY_CURRENT_GROUP, groupObject);

        currentUser.saveEventually(e -> {
            Toast.makeText(mContext, "Club updated successfully", Toast.LENGTH_SHORT).show();

            // Return to MainActivity
            Intent intent = new Intent(mContext, MainActivity.class);
            intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
            mContext.startActivity(intent);

            // Update Installation object for notifications
            updateParseInstallation(ParseUser.getCurrentUser());
        });
    }


    private void removeGroup(ParseObject groupObject) {
        ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
        userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
        userQuery.findInBackground((users, e) -> {
            if (e == null) for (ParseObject userObject : users) {

                List<string> myGroups = userObject.getList(ParseConstants.KEY_MY_GROUPS);
                myGroups.Remove(groupObject.getObjectId());
                userObject.Add(ParseConstants.KEY_MY_GROUPS, myGroups);
                userObject.saveInBackground(arg0 -> {
                    Toast.makeText(mContext, "Club deleted successfully", Toast.LENGTH_SHORT).show();

                    Intent intent = new Intent(mContext, MainActivity.class);
                    intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                    mContext.startActivity(intent);
                });

                // Update Installation object for notifications
                updateParseInstallation(ParseUser.getCurrentUser());
            }
        });
    }


    private void updateParseInstallation(ParseUser user) {

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
                    installation.Add("GCMSenderId", com.parse.ui.R.string.gcm_sender_id);
                    installation.Add("userId", user.getObjectId());
                    installation.saveInBackground();

                }
            }
        });

    }


    public class ViewHolder : RecyclerView.ViewHolder {
        TextView groupName;
        TextView groupDescription;
        ImageView groupProfilePicture;

        public ViewHolder(View itemView) {
            base(itemView);

            groupName = (TextView) itemView.findViewById(R.id.groupName);
            groupDescription = (TextView) itemView.findViewById(R.id.groupDescription);
            groupProfilePicture = (ImageView) itemView.findViewById(R.id.groupProfilePicture);
        }
    }

}
}
