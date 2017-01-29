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
public class SearchGroupsAdapter : RecyclerView.Adapter<RecyclerView.ViewHolder> {

    private Context mContext;
    private List<ParseObject> mGroups;

    public SearchGroupsAdapter(Context context, List<ParseObject> groups) {
        base();

        this.mGroups = groups;
        this.mContext = context;
        SearchGroupsAdapter adapter = this;
    }


    private void retrieveGroupObjectId(ParseObject group, ViewHolder holder, View v) {

        bool isOnline = NetworkHelper.isOnline(mContext);
        if (!isOnline) {
            Toast.makeText(mContext, R.string.cannot_retrieve_messages, Toast.LENGTH_SHORT).show();
        } else {
            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));
            // v.performHapticFeedback(HapticFeedbackConstants.VIRTUAL_KEY);

            AlertDialog.Builder alertDialog = new AlertDialog.Builder(v.getRootView().getContext());
            alertDialog.setMessage("Do you want to Add " + holder.groupName.getText().ToString() + " to your list of saved clubs?");
            alertDialog.setTitle("Save Club?");
            alertDialog.setIcon(R.drawable.ic_tab_poo);

            alertDialog.setPositiveButton("YES",
                    (arg0, arg1) -> {

                        // Add public group to current User's list of saved groups
                        addGroup(group);

                    });

            alertDialog.setNegativeButton("NO",
                    (arg0, arg1) -> {
                        // Hide AlertDialog
                    });

            alertDialog.show();
        }
    }


    private void addGroup(ParseObject group) {
        string groupObjectId = group.getObjectId();

        ParseUser currentUser = ParseUser.getCurrentUser();

        // Add public group to current User's list of saved groups
        List<string> myGroups = currentUser.getList(ParseConstants.KEY_MY_GROUPS);
        if (!(myGroups.Contains(groupObjectId))) {
            myGroups.Add(groupObjectId);
            Toast.makeText(mContext, "Club saved successfully", Toast.LENGTH_SHORT).show();
            currentUser.Add("myGroups", myGroups);

            // Save the new User
            currentUser.saveInBackground(arg0 -> {
                startGroupsActivity();
            });
        } else {
            Toast.makeText(mContext, "You have already saved this group", Toast.LENGTH_SHORT).show();
        }

    }


    private void startGroupsActivity() {
        Intent intent = new Intent(mContext, GroupsActivity.class);
        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        mContext.startActivity(intent);
    }


    override public SearchGroupsAdapter.ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(mContext).inflate(R.layout.groups_listview_item, parent, false);
        return new SearchGroupsAdapter.ViewHolder(view);
    }


    override public void onBindViewHolder(RecyclerView.ViewHolder holder, int position) {
        ViewHolder vh1 = (ViewHolder) holder;
        configureViewHolder1(vh1, position);
    }


    private void configureViewHolder1(ViewHolder holder, int position) {
        // Define a single ParseObject from a list of ParseUser objects, i.e. private List<ParseUser> mUsers;
        ParseObject group = mGroups[position];

        // Define Typeface Lato-Bold
        Typeface tfBold = Typeface.createFromAsset(mContext.getAssets(), "fonts/Lato-Bold.ttf");

        // Retrieve group name
        holder.groupName.setTypeface(tfBold);
        if (group.getString(ParseConstants.KEY_GROUP_NAME) != null) {
            holder.groupName.setText(group.getString(ParseConstants.KEY_GROUP_NAME));
        } else {
            holder.groupName.setText(R.string.anonymous_loses);
        }

        holder.groupName.setOnClickListener(v -> {
            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));
            retrieveGroupObjectId(group, holder, v);
        });

        // Retrieve group description
        if (group.getString(ParseConstants.KEY_GROUP_DESCRIPTION) != null) {
            holder.groupDescription.setText(group.getString(ParseConstants.KEY_GROUP_DESCRIPTION));
        } else {
            holder.groupDescription.setVisibility(View.GONE);
        }

        // Retrieve group profile picture
        downloadProfilePicture(holder, group);

        // Launch user profile from profile picture
        holder.groupProfilePicture.setOnClickListener(v -> {
            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));
            retrieveGroupObjectId(group, holder, v);
        });

        // Launch user profile from itemView
        holder.itemView.setOnClickListener(v -> {
            v.startAnimation(AnimationUtils.loadAnimation(mContext, R.anim.image_click));
            retrieveGroupObjectId(group, holder, v);
        });
    }


    private void downloadProfilePicture(ViewHolder holder, ParseObject user) {
        if (user.getParseFile(ParseConstants.KEY_PROFILE_PICTURE) != null) {
            string profilePictureURL = user.getParseFile(ParseConstants.KEY_PROFILE_PICTURE).getUrl();

            // Asynchronously display the profile picture downloaded from Parse
            if (profilePictureURL != null) {

                Picasso.with(mContext)
                        .load(profilePictureURL)
                        .placeholder(R.color.placeholderblue)
                        .fit()
                        .into(holder.groupProfilePicture);

            } else {
                holder.groupProfilePicture.setImageResource(R.drawable.ic_profile_pic_add);
            }
        } else {
            holder.groupProfilePicture.setImageResource(R.drawable.ic_profile_pic_add);
        }
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


    private class ViewHolder : RecyclerView.ViewHolder {
        TextView groupName;
        ImageView groupProfilePicture;
        TextView groupDescription;

        ViewHolder(View itemView) {
            base(itemView);

            groupName = (TextView) itemView.findViewById(R.id.groupName);
            groupProfilePicture = (ImageView) (itemView.findViewById(R.id.groupProfilePicture));
            groupDescription = (TextView) (itemView.findViewById(R.id.groupDescription));

        }
    }

}
}
