using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpLanguageTool.Extensions;

namespace com.yeetclub.android.fragment
{

/// <summary>
/// Created by @santafebound on 2016-09-29.
/// </summary>




public class UsersListFragment : Fragment {

    private const string TAG = "UsersListFragment";
    private const string KEY_LAYOUT_MANAGER = "layoutManager";

    private enum LayoutManagerType {
        LINEAR_LAYOUT_MANAGER
    }

    protected LayoutManagerType mCurrentLayoutManagerType;

    protected RecyclerView mRecyclerView;
    protected RecyclerView.LayoutManager mLayoutManager;

    protected List<ParseUser> mUsers;
    protected PullToRefreshView mSwipeRefreshLayout;

    public UsersListFragment() {

    }

    override public void onCreate(Bundle savedInstanceState) {
        base.onCreate(savedInstanceState);
    }

    override public void onResume() {
        base.onResume();  // Always call the superclass method first

        // Is the network online?
        bool isOnline = NetworkHelper.isOnline(getContext());

        // Initialize dataset from remote server
        retrieveUsers(isOnline);
    }

    override public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {

        // HashSet view
        View rootView = inflater.inflate(R.layout.tab_fragment_1, container, false);
        rootView.setTag(TAG);

        // Initialize SwipeRefreshLayout
        mSwipeRefreshLayout = (PullToRefreshView) rootView.findViewById(R.id.swipeRefreshLayout);

        // Is the network online?
        bool isOnline = NetworkHelper.isOnline(getContext());

        rootView.setFocusableInTouchMode(true);
        rootView.requestFocus();

        // Return to first fragment on back press
        rootView.setOnKeyListener((v, keyCode, event) -> {
            if (event.getAction() == KeyEvent.ACTION_DOWN) {
                if (keyCode == KeyEvent.KEYCODE_BACK) {
                    ViewPager viewPager = (ViewPager) getActivity().findViewById(R.id.pager);
                    viewPager.setCurrentItem(0, true);
                    return true;
                }
            }
            return false;
        });

        retrieveUsers(isOnline);

        mRecyclerView = (RecyclerView) rootView.findViewById(R.id.recyclerView);

        // LinearLayoutManager is used here, this will layout the elements in a similar fashion
        // to the way ListView would layout elements. The RecyclerView.LayoutManager defines how
        // elements are laid out.
        mLayoutManager = new LinearLayoutManager(getActivity());

        mCurrentLayoutManagerType = LayoutManagerType.LINEAR_LAYOUT_MANAGER;

        if (savedInstanceState != null) {
            // Restore saved layout manager type.
            mCurrentLayoutManagerType = (LayoutManagerType) savedInstanceState
                    .getSerializable(KEY_LAYOUT_MANAGER);
        }
        setRecyclerViewLayoutManager(mCurrentLayoutManagerType);

        setRecyclerViewLayoutManager(LayoutManagerType.LINEAR_LAYOUT_MANAGER);

        return rootView;
    }

    public void setRecyclerViewLayoutManager(LayoutManagerType layoutManagerType) {
        int scrollPosition = 0;

        // If a layout manager has already been set, get current scroll position.
        if (mRecyclerView.getLayoutManager() != null) {
            scrollPosition = ((LinearLayoutManager) mRecyclerView.getLayoutManager())
                    .findFirstCompletelyVisibleItemPosition();
        }

        switch (layoutManagerType) {
            case LINEAR_LAYOUT_MANAGER:
                mLayoutManager = new LinearLayoutManager(getActivity());
                mCurrentLayoutManagerType = LayoutManagerType.LINEAR_LAYOUT_MANAGER;
                break;
            default:
                mLayoutManager = new LinearLayoutManager(getActivity());
                mCurrentLayoutManagerType = LayoutManagerType.LINEAR_LAYOUT_MANAGER;
        }

        mRecyclerView.setLayoutManager(mLayoutManager);
        mRecyclerView.scrollToPosition(scrollPosition);
        mRecyclerView.addItemDecoration(new DividerItemDecoration(getActivity(), DividerItemDecoration.VERTICAL_LIST));
    }

    override public void onSaveInstanceState(Bundle savedInstanceState) {
        // Save currently selected layout manager.
        savedInstanceState.putSerializable(KEY_LAYOUT_MANAGER, mCurrentLayoutManagerType);
        base.onSaveInstanceState(savedInstanceState);
    }

    private void retrieveUsers(bool isOnline) {
        ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
        // Look for self!
        userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
        userQuery.findInBackground((users, e) -> {
            if (e == null) for (ParseObject userObject : users) {
                // Which group am I currently in?
                string currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP).getObjectId();
                List<string> theirGroups = userObject.getList(ParseConstants.KEY_MY_GROUPS);

                // Log.w(TAG, currentGroupObjectId);
                // Log.w(TAG, string.valueOf(theirGroups));

                ParseQuery<ParseUser> userQuery2 = ParseUser.getQuery();
                // Look for other users within my current group only
                userQuery2.whereContains(ParseConstants.KEY_MY_GROUPS, currentGroupObjectId);
                userQuery2.addAscendingOrder(ParseConstants.KEY_USERNAME);
                if (!isOnline) {
                    userQuery2.fromLocalDatastore();
                }
                userQuery2.findInBackground((users2, e2) -> {

                    mSwipeRefreshLayout.setRefreshing(false);

                    if (e2 == null) {

                        // We found messages!
                        mUsers = users2;
                        ParseObject.pinAllInBackground(mUsers);
                        /*Console.WriteLine(yeets);*/

                        UsersListAdapter adapter = new UsersListAdapter(getContext(), users2);
                        adapter.setHasStableIds(true);
                        mRecyclerView.setHasFixedSize(true);
                        adapter.notifyDataSetChanged();
                        mRecyclerView.setAdapter(adapter);

                        mSwipeRefreshLayout.setOnRefreshListener(() -> {
                            if (!isOnline) {
                                mSwipeRefreshLayout.setRefreshing(false);
                                Toast.makeText(getContext(), getString(R.string.cannot_retrieve_messages), Toast.LENGTH_SHORT).show();
                            } else {
                                retrieveUsers(true);
                            }
                        });
                    }
                });
            }
        });
    }
}
}
