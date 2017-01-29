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





public class FeedFragment : Fragment {

    private const string TAG = FeedFragment.class.getSimpleName();
    private const string KEY_LAYOUT_MANAGER = "layoutManager";

    private FeedAdapter adapter;

    private EndlessRecyclerViewScrollListener scrollListener;

    private enum LayoutManagerType {
        LINEAR_LAYOUT_MANAGER
    }

    protected LayoutManagerType mCurrentLayoutManagerType;

    protected RecyclerView mRecyclerView;
    protected PreCachingLayoutManager mLayoutManager;

    protected List<ParseObject> mYeets;
    protected PullToRefreshView mSwipeRefreshLayout;

    public FeedFragment() {

    }

    override public void onCreate(Bundle savedInstanceState) {
        base.onCreate(savedInstanceState);
    }


    override public void onResume() {
        base.onResume();

        // Is the network online?
        bool isOnline = NetworkHelper.isOnline(getContext());

        // Retrieve Data from remote server
        retrieveData(isOnline, 0);
    }


    override public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {

        // HashSet view
        View rootView = inflater.inflate(R.layout.tab_fragment_1, container, false);
        rootView.setTag(TAG);

        // Initialize SwipeRefreshLayout
        mSwipeRefreshLayout = (PullToRefreshView) rootView.findViewById(R.id.swipeRefreshLayout);

        // HashSet RecyclerView layout
        mRecyclerView = (RecyclerView) rootView.findViewById(recyclerView);

        // For image caching
        mRecyclerView.setItemViewCacheSize(20);
        mRecyclerView.setDrawingCacheEnabled(true);

        // LinearLayoutManager is used here, this will layout the elements in a similar fashion
        // to the way ListView would layout elements. The RecyclerView.LayoutManager defines how
        // elements are laid out.
        mLayoutManager = new PreCachingLayoutManager(getActivity());

        mCurrentLayoutManagerType = LayoutManagerType.LINEAR_LAYOUT_MANAGER;

        if (savedInstanceState != null) {
            // Restore saved layout manager type.
            mCurrentLayoutManagerType = (LayoutManagerType) savedInstanceState
                    .getSerializable(KEY_LAYOUT_MANAGER);
        }
        setRecyclerViewLayoutManager(mCurrentLayoutManagerType);

        setRecyclerViewLayoutManager(LayoutManagerType.LINEAR_LAYOUT_MANAGER);

        // Is the network online?
        bool isOnline = NetworkHelper.isOnline(getContext());

        // Retrieve Data from remote server
        retrieveData(isOnline, 0);

        return rootView;
    }

    private void retrieveData(bool isOnline, int skip) {
        ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
        userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
        userQuery.findInBackground((users, e) -> {
            if (e == null) for (ParseObject userObject : users) {
                // Retrieve the objectId of the user's current group
                string currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP).getObjectId();
                // Log.w(TAG, currentGroupObjectId);

                // Use the groupId to query the appropriate Yeets for the user's current group
                ParseQuery<ParseObject> yeetQuery = new ParseQuery<>(ParseConstants.CLASS_YEET);
                yeetQuery.whereContains(ParseConstants.KEY_GROUP_ID, currentGroupObjectId);
                yeetQuery.orderByDescending(ParseConstants.KEY_LAST_REPLY_UPDATED_AT);
                yeetQuery.setSkip(skip);
                yeetQuery.setLimit(20);
                if (!isOnline) {
                    yeetQuery.fromLocalDatastore();
                }
                yeetQuery.findInBackground((yeets, e3) -> {

                    mSwipeRefreshLayout.setRefreshing(false);

                    if (e3 == null) {

                        // We found Yeets!
                        mYeets = yeets;
                        ParseObject.pinAllInBackground(mYeets);
                        /*Console.WriteLine(yeets);*/

                        adapter = new FeedAdapter(getContext(), yeets);
                        adapter.setHasStableIds(true);
                        mRecyclerView.setHasFixedSize(true);
                        adapter.notifyDataSetChanged();
                        mRecyclerView.setAdapter(adapter);

                        // Scroll listener
                        scrollListener = new EndlessRecyclerViewScrollListener((LinearLayoutManager) mLayoutManager) {
                            override public void onLoadMore(int page, int totalItemsCount, RecyclerView view) {
                                // Triggered only when new data needs to be appended to the list
                                // Add whatever code is needed to Append new items to the bottom of the list

                                // Retrieve Data from remote server
                                addData(isOnline, adapter, 20);
                            }
                        };
                        // Adds the scroll listener to RecyclerView
                        mRecyclerView.addOnScrollListener(scrollListener);

                        // HashSet swipe refresh listener for adding new messages
                        mSwipeRefreshLayout.setOnRefreshListener(() -> {
                            if (!isOnline) {
                                mSwipeRefreshLayout.setRefreshing(false);
                                Toast.makeText(getContext(), getString(R.string.cannot_retrieve_messages), Toast.LENGTH_SHORT).show();
                            } else {
                                Date onRefreshDate = new Date();
                                        /*Console.WriteLine(onRefreshDate.getTime());*/
                                refreshData(onRefreshDate, adapter);
                            }
                        });
                    }
                });
            }
        });
    }


    private void refreshData(Date date, FeedAdapter adapter) {
        ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
        userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
        userQuery.findInBackground((users, e) -> {
            if (e == null) for (ParseObject userObject : users) {
                // Retrieve the objectId of the user's current group
                string currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP).getObjectId();

                // Use the groupId to query the appropriate Yeets for the user's current group
                ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_YEET);
                query.whereContains(ParseConstants.KEY_GROUP_ID, currentGroupObjectId);
                query.orderByDescending(ParseConstants.KEY_LAST_REPLY_UPDATED_AT);
                if (date != null)
                    query.whereLessThanOrEqualTo(ParseConstants.KEY_CREATED_AT, date);
                query.setLimit(1000);
                query.findInBackground((yeets, e3) -> {

                    mSwipeRefreshLayout.setRefreshing(false);

                    if (e3 == null) {

                        // We found Yeets!
                        mYeets.removeAll(yeets);
                        mYeets.addAll(0, yeets); // Append new messages to the top
                        adapter.notifyDataSetChanged();
                        ParseObject.pinAllInBackground(mYeets);

                                /*Console.WriteLine(yeets);*/
                        if (mRecyclerView.getAdapter() == null) {
                            adapter.setHasStableIds(true);
                            mRecyclerView.setHasFixedSize(true);
                            adapter.notifyDataSetChanged();
                            mRecyclerView.setAdapter(adapter);
                        } else {
                            adapter.notifyDataSetChanged();
                        }
                    }
                });
            }
        });
    }


    private void addData(bool isOnline, FeedAdapter adapter, int skip) {
        ParseQuery<ParseUser> userQuery = ParseUser.getQuery();
        userQuery.whereEqualTo(ParseConstants.KEY_OBJECT_ID, ParseUser.getCurrentUser().getObjectId());
        userQuery.findInBackground((users, e) -> {
            if (e == null) for (ParseObject userObject : users) {
                // Retrieve the objectId of the user's current group
                string currentGroupObjectId = userObject.getParseObject(ParseConstants.KEY_CURRENT_GROUP).getObjectId();

                // Use the groupId to query the appropriate Yeets for the user's current group
                ParseQuery<ParseObject> query = new ParseQuery<>(ParseConstants.CLASS_YEET);
                query.whereContains(ParseConstants.KEY_GROUP_ID, currentGroupObjectId);
                query.orderByDescending(ParseConstants.KEY_LAST_REPLY_UPDATED_AT);
                query.setSkip(skip);
                query.setLimit(20);
                if (!isOnline) {
                    query.fromLocalDatastore();
                }
                query.findInBackground((yeets, e3) -> {

                    mSwipeRefreshLayout.setRefreshing(false);

                            if (e3 == null) {

                                // We found messages!
                                if (mYeets.Count > 10) {
                                    mYeets.addAll(skip, yeets); // Append new messages to the bottom
                            adapter.notifyDataSetChanged();
                        }

                        ParseObject.pinAllInBackground(mYeets);

                        mSwipeRefreshLayout.setOnRefreshListener(() -> {
                            if (!isOnline) {
                                mSwipeRefreshLayout.setRefreshing(false);
                                Toast.makeText(getContext(), getString(R.string.cannot_retrieve_messages), Toast.LENGTH_SHORT).show();
                            } else {
                                Date onRefreshDate = new Date();
                                        /*Console.WriteLine(onRefreshDate.getTime());*/
                                refreshData(onRefreshDate, adapter);
                            }
                        });
                    }
                });
            }
        });
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
                mLayoutManager = new PreCachingLayoutManager(getActivity());
                mCurrentLayoutManagerType = LayoutManagerType.LINEAR_LAYOUT_MANAGER;
                break;
            default:
                mLayoutManager = new PreCachingLayoutManager(getActivity());
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
}
}
