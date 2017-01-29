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




public class NotificationsFragment : Fragment {

    private const string TAG = "NotificationsFragment";
    private const string KEY_LAYOUT_MANAGER = "layoutManager";

    private enum LayoutManagerType {
        LINEAR_LAYOUT_MANAGER
    }

    protected LayoutManagerType mCurrentLayoutManagerType;

    protected RecyclerView mRecyclerView;
    protected RecyclerView.LayoutManager mLayoutManager;

    protected List<ParseObject> mNotifications;
    protected PullToRefreshView mSwipeRefreshLayout;

    public NotificationsFragment() {

    }

    override public void onCreate(Bundle savedInstanceState) {
        base.onCreate(savedInstanceState);
    }


    override public void onResume() {
        base.onResume();

        // Is the network online?
        bool isOnline = NetworkHelper.isOnline(getContext());

        // Retrieve Data from remote server
        retrieveNotifications(isOnline);
    }


    override public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {

        // HashSet view
        View rootView = inflater.inflate(R.layout.tab_fragment_2, container, false);
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

        if (!isOnline) {
            mSwipeRefreshLayout.setRefreshing(false);
            Toast.makeText(getContext(), getString(R.string.cannot_retrieve_messages), Toast.LENGTH_SHORT).show();
        } else {
            // Retrieve new notifications
            retrieveNotifications(rootView, true);
        }

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


    private void setReadState() {
        ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_NOTIFICATIONS);
        query.whereEqualTo(ParseConstants.KEY_RECIPIENT_ID, ParseUser.getCurrentUser().getObjectId());
        query.whereEqualTo(ParseConstants.KEY_READ_STATE, false);
        query.findInBackground((notifications, e) -> {

            mSwipeRefreshLayout.setRefreshing(false);

            if (e == null) {

                foreach (ParseObject notificationObject in notifications) {
                    // Console.WriteLine(notificationObject);
                    notificationObject.Add(ParseConstants.KEY_READ_STATE, true);
                    notificationObject.saveEventually();

                }

            } else {
                Toast.makeText(getContext(), R.string.could_not_retrieve, Toast.LENGTH_SHORT).show();
                /*Log.w(getContext().ToString(), e);*/
            }
        });
    }


    private void showViews(View rootView) {
        rootView.findViewById(R.id.noNotificationsMessage).setVisibility(View.VISIBLE);
        rootView.findViewById(R.id.noNotificationsMessage2).setVisibility(View.VISIBLE);
        rootView.findViewById(R.id.noNotificationsImage).setVisibility(View.VISIBLE);
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


    private void retrieveNotifications(View rootView, Boolean isOnline) {
        ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
        userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
        userQuery.findInBackground((users, e) -> {
            if (e == null) for (ParseObject userObject : users) {
                // Retrieve the objectId of the user's current group
                string currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP).getObjectId();
                //Log.w(TAG, currentGroupObjectId);

                ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_NOTIFICATIONS);
                query.whereEqualTo(ParseConstants.KEY_RECIPIENT_ID, ParseUser.getCurrentUser().getObjectId());
                query.whereContains(ParseConstants.KEY_GROUP_ID, currentGroupObjectId);
                query.addDescendingOrder(ParseConstants.KEY_CREATED_AT);
                if (!isOnline) {
                    query.fromLocalDatastore();
                }
                query.findInBackground((notifications, e2) -> {

                    mSwipeRefreshLayout.setRefreshing(false);

                    if (e2 == null) {

                        // We found messages!
                        mNotifications = notifications;
                        ParseObject.pinAllInBackground(mNotifications);

                        if (notifications.IsEmpty()) {
                            showViews(rootView);
                        }

                        NotificationsAdapter adapter = new NotificationsAdapter(getContext(), notifications);
                        adapter.setHasStableIds(true);
                        mRecyclerView.setHasFixedSize(true);
                        // mRecyclerView.addItemDecoration(new DividerItemDecoration(getActivity(), DividerItemDecoration.VERTICAL_LIST));
                        adapter.notifyDataSetChanged();
                        mRecyclerView.setAdapter(adapter);

                        mSwipeRefreshLayout.setOnRefreshListener(() -> {
                            if (!isOnline) {
                                mSwipeRefreshLayout.setRefreshing(false);
                                Toast.makeText(getContext(), getString(R.string.cannot_retrieve_messages), Toast.LENGTH_SHORT).show();
                            } else {
                                // HashSet all current notifications to "read"
                                setReadState();

                                Date onRefreshDate = new Date();
                                /*Console.WriteLine(onRefreshDate.getTime());*/
                                refreshYeets(onRefreshDate, adapter, true);
                            }
                        });

                    }
                });
            }
        });
    }


    private void retrieveNotifications(Boolean isOnline) {
        ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
        userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
        userQuery.findInBackground((users, e) -> {
            if (e == null) for (ParseObject userObject : users) {
                // Retrieve the objectId of the user's current group
                string currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP).getObjectId();
                //Log.w(TAG, currentGroupObjectId);

                ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_NOTIFICATIONS);
                query.whereEqualTo(ParseConstants.KEY_RECIPIENT_ID, ParseUser.getCurrentUser().getObjectId());
                query.whereContains(ParseConstants.KEY_GROUP_ID, currentGroupObjectId);
                query.addDescendingOrder(ParseConstants.KEY_CREATED_AT);
                if (!isOnline) {
                    query.fromLocalDatastore();
                }
                query.findInBackground((notifications, e2) -> {

                    mSwipeRefreshLayout.setRefreshing(false);

                    if (e2 == null) {

                        // We found messages!
                        mNotifications = notifications;
                        ParseObject.pinAllInBackground(mNotifications);

                        NotificationsAdapter adapter = new NotificationsAdapter(getContext(), notifications);
                        adapter.setHasStableIds(true);
                        mRecyclerView.setHasFixedSize(true);
                        // mRecyclerView.addItemDecoration(new DividerItemDecoration(getActivity(), DividerItemDecoration.VERTICAL_LIST));
                        adapter.notifyDataSetChanged();
                        mRecyclerView.setAdapter(adapter);

                        mSwipeRefreshLayout.setOnRefreshListener(() -> {
                            if (!isOnline) {
                                mSwipeRefreshLayout.setRefreshing(false);
                                Toast.makeText(getContext(), getString(R.string.cannot_retrieve_messages), Toast.LENGTH_SHORT).show();
                            } else {
                                // HashSet all current notifications to "read"
                                setReadState();

                                Date onRefreshDate = new Date();
                                /*Console.WriteLine(onRefreshDate.getTime());*/
                                refreshYeets(onRefreshDate, adapter, true);
                            }
                        });

                    }
                });
            }
        });
    }


    private void refreshYeets(Date date, NotificationsAdapter adapter, Boolean isOnline) {
        ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
        userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
        userQuery.findInBackground((users, e) -> {
            if (e == null) for (ParseObject userObject : users) {
                // Retrieve the objectId of the user's current group
                string currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP).getObjectId();
                //Log.w(TAG, currentGroupObjectId);

                ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_NOTIFICATIONS);
                query.whereEqualTo(ParseConstants.KEY_RECIPIENT_ID, ParseUser.getCurrentUser().getObjectId());
                query.whereContains(ParseConstants.KEY_GROUP_ID, currentGroupObjectId);
                query.addDescendingOrder(ParseConstants.KEY_CREATED_AT);
                if (date != null)
                    query.whereLessThanOrEqualTo(ParseConstants.KEY_CREATED_AT, date);
                query.setLimit(1000);
                query.findInBackground((notifications, e2) -> {

                    mSwipeRefreshLayout.setRefreshing(false);

                    if (e2 == null) {
                        // We found messages!
                        mNotifications.removeAll(notifications);
                        mNotifications.addAll(0, notifications); //This should Append new messages to the top
                        adapter.notifyDataSetChanged();
                        ParseObject.pinAllInBackground(mNotifications);

                        /*Console.WriteLine(yeets);*/
                        if (mRecyclerView.getAdapter() == null) {
                            adapter.setHasStableIds(true);
                            mRecyclerView.setHasFixedSize(true);
                            adapter.notifyDataSetChanged();
                            mRecyclerView.setAdapter(adapter);
                        } else {
                            adapter.notifyDataSetChanged();
                        }

                        mSwipeRefreshLayout.setOnRefreshListener(() -> {
                            if (!isOnline) {
                                mSwipeRefreshLayout.setRefreshing(false);
                                Toast.makeText(getContext(), getString(R.string.cannot_retrieve_messages), Toast.LENGTH_SHORT).show();
                            } else {
                                // HashSet all current notifications to "read"
                                setReadState();

                                Date onRefreshDate = new Date();
                                /*Console.WriteLine(onRefreshDate.getTime());*/
                                refreshYeets(onRefreshDate, adapter, true);
                            }
                        });
                    }
                });

            }
        });


    }
}
}
