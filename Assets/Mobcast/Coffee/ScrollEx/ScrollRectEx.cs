﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using EnhancedUI;
using UnityEngine.EventSystems;

//using TweenType = ScrollTweener.TweenType;
using Method = Mobcast.Coffee.Tweening.Method;

namespace Mobcast.Coffee
{
//	/// <summary>
//	/// This delegate handles the visibility changes of cell views
//	/// </summary>
//	/// <param name="cellView">The cell view that changed visibility</param>
//	public delegate void CellViewVisibilityChangedDelegate(ScrollCellView cellView);
//
//	/// <summary>
//	/// This delegate will be fired just before the cell view is recycled
//	/// </summary>
//	/// <param name="cellView"></param>
//	public delegate void CellViewWillRecycleDelegate(ScrollCellView cellView);

	/// <summary>
	/// The ScrollRectEx allows you to easily set up a dynamic scroller that will recycle views for you. This means
	/// that using only a handful of views, you can display thousands of rows. This will save memory and processing
	/// power in your application.
	/// </summary>
	[RequireComponent(typeof(ScrollRect))]
	public class ScrollRectEx : MonoBehaviour, IScrollSnap
	{
		[SerializeField]
		ScrollSnap m_ScrollSnap = new ScrollSnap();


		/// <summary>
		/// Gets the number of cells in a list of data
		/// </summary>
		/// <param name="scroller"></param>
		/// <returns></returns>
		//		public virtual int GetNumberOfCells(ScrollRectEx scroller)
		//		{
		//			return 0;
		//		}

		/// <summary>
		/// Gets the size of a cell view given the index of the data set.
		/// This allows you to have different sized cells
		/// </summary>
		/// <param name="scroller"></param>
		/// <param name="dataIndex"></param>
		/// <returns></returns>
		public virtual float GetCellViewSize(int dataIndex)
		{
			return 0;
		}

		/// <summary>
		/// Gets the cell view that should be used for the data index. Your implementation
		/// of this function should request a new cell from the scroller so that it can
		/// properly recycle old cells.
		/// </summary>
		/// <param name="scroller"></param>
		/// <param name="dataIndex"></param>
		/// <param name="cellIndex"></param>
		/// <returns></returns>
		public virtual ScrollCellView GetCellView(int dataIndex, int cellIndex)
		{
			return null;
		}

		/// <summary>
		/// This delegate handles the visibility changes of cell views
		/// </summary>
		/// <param name="cellView">The cell view that changed visibility</param>
		public virtual void OnChangedCellViewVisibility(ScrollCellView cellView)
		{
		}

		/// <summary>
		/// This delegate will be fired just before the cell view is recycled
		/// </summary>
		/// <param name="cellView"></param>
		public virtual void OnWillRecycleCellView(ScrollCellView cellView)
		{
		}



		#region Public

		/// <summary>
		/// Which side of a cell to reference.
		/// For vertical scrollers, before means above, after means below.
		/// For horizontal scrollers, before means to left of, after means to the right of.
		/// </summary>
		//		public enum CellViewPositionEnum
		//		{
		//			Before,
		//			After
		//		}

		/// <summary>
		/// The direction the scroller is handling
		/// </summary>
		//		public ScrollDirectionEnum scrollDirection;

		/// <summary>
		/// The spacing to use between layout elements in the layout group.
		/// </summary>
		public float spacing { get { return layoutGroup.spacing; } }

		/// <summary>
		/// The padding to add around the child layout elements.
		/// </summary>
		public RectOffset padding { get { return layoutGroup.padding; } }

		/// <summary>
		/// Whether the scroller should loop the cell views.
		/// </summary>
		[SerializeField]
		private bool m_Loop;

		/// <summary>
		/// Whether the scollbar should be shown
		/// </summary>
		//		[SerializeField]
		private ScrollRect.ScrollbarVisibility _scrollbarVisibility;

//		/// <summary>
//		/// This delegate is called when a cell view is hidden or shown
//		/// </summary>
//		public CellViewVisibilityChangedDelegate cellViewVisibilityChanged;
//
//		/// <summary>
//		/// This delegate is called just before a cell view is hidden by recycling
//		/// </summary>
//		public CellViewWillRecycleDelegate cellViewWillRecycle;

		/// <summary>
		/// The absolute position in pixels from the start of the scroller
		/// </summary>
		public float ScrollPosition
		{
			get { return _scrollPosition; }
			set
			{
				// 非ループ時はオーバーランを防ぐためにスクロール制限をかけます.
				if (!loop)
				{
					var min = -scrollRectSize;
					var max = GetScrollPositionForCellViewIndex(_cellViewSizeArray.Count - 1) + scrollRectSize;
					if (value < min || max < value)
					{
						value = Mathf.Clamp(value, min, max);
						scrollRect.velocity = Vector2.zero;
					}
				}

				// 座標が変更された時のみ、新しく座標を設定します.
				if (0.01f < Mathf.Abs(_scrollPosition - value))
				{
					m_NeedRefleshActive = true;
					_scrollPosition = value;
					if (m_ScrollRect.vertical)
						m_ScrollRect.verticalNormalizedPosition = 1f - (_scrollPosition / scrollSize);
					else
						m_ScrollRect.horizontalNormalizedPosition = (_scrollPosition / scrollSize);
				}
			}
		}

		/// <summary>
		/// Whether the scroller should loop the resulting cell views.
		/// Looping creates three sets of internal size data, attempting
		/// to keep the scroller in the middle set. If the scroller goes
		/// outside of this set, it will jump back into the middle set,
		/// giving the illusion of an infinite set of data.
		/// </summary>
		public bool loop
		{
			get { return m_Loop; }
			set
			{
				if (m_Loop == value)
					return;
				
				// only if the value has changed
//				if (loop != value)
//				{
				m_Loop = value;

				// get the original position so that when we turn looping on
				// we can jump back to this position
				// 
				var oldPos = _scrollPosition;


				// call resize to generate more internal elements if loop is on,
				// remove the elements if loop is off
				_Resize(false);
				ScrollPosition = m_Loop
					? _loopFirstScrollPosition + oldPos
					: oldPos - _loopFirstScrollPosition;
//					if (loop)
//					{
//						// set the new scroll position based on the middle set of data + the original position
//						ScrollPosition = _loopFirstScrollPosition + oldPos;
//					}
//					else
//					{
//						// set the new scroll position based on the original position and the first loop position
//						ScrollPosition = oldPos - _loopFirstScrollPosition;
//					}

				// update the scrollbars
				ScrollbarVisibility = _scrollbarVisibility;
//				}
			}
		}

		/// <summary>
		/// Sets how the visibility of the scrollbars should be handled
		/// </summary>
		public ScrollRect.ScrollbarVisibility ScrollbarVisibility
		{
			get { return _scrollbarVisibility; }
			set
			{
				_scrollbarVisibility = value;
				if (!scrollbar)
					return;

				// only if the scrollbar exists
//				if (scrollbar != null)
//				{
				// make sure we actually have some cell views
				if (_cellViewOffsetArray != null && _cellViewOffsetArray.Count > 0)
				{
					if (_cellViewOffsetArray.Last() < scrollRectSize || loop)
					{
						// if the size of the scrollable area is smaller than the scroller
						// or if we have looping on, hide the scrollbar unless the visibility
						// is set to Always.
						scrollbar.gameObject.SetActive(_scrollbarVisibility == ScrollRect.ScrollbarVisibility.Permanent);
					}
					else
					{
						// if the size of the scrollable areas is larger than the scroller
						// or looping is off, then show the scrollbars unless visibility
						// is set to Never.
						scrollbar.gameObject.SetActive(true);
					}
				}
//				}
			}
		}


		/// <summary>
		/// This is the first cell view index showing in the scroller's visible area
		/// </summary>
		public int StartCellViewIndex { get { return _activeCellViewsStartIndex; } }

		/// <summary>
		/// This is the last cell view index showing in the scroller's visible area
		/// </summary>
		public int EndCellViewIndex { get { return _activeCellViewsEndIndex; } }

		/// <summary>
		/// This is the first data index showing in the scroller's visible area
		/// </summary>
		public int StartDataIndex { get { return _activeCellViewsStartIndex % cellCount; } }

		/// <summary>
		/// This is the last data index showing in the scroller's visible area
		/// </summary>
		public int EndDataIndex { get { return _activeCellViewsEndIndex % cellCount; } }

		/// <summary>
		/// This is the number of cells in the scroller
		/// </summary>
		public virtual int cellCount { get { return -1; } }


		/// <summary>
		/// The size of the visible portion of the scroller
		/// </summary>
		public float scrollRectSize { get { return scrollRect.vertical ? m_ScrollRectTransform.rect.height : m_ScrollRectTransform.rect.width; } }
		//		{
		//			get
		//			{
		//				if (scrollRect.vertical)
		//					return m_ScrollRectTransform.rect.height;
		//				else
		//					return m_ScrollRectTransform.rect.width;
		//			}
		//		}

		/// <summary>
		/// Create a cell view, or recycle one if it already exists
		/// </summary>
		/// <param name="cellPrefab">The prefab to use to create the cell view</param>
		/// <returns></returns>
		public ScrollCellView GetCellView(ScrollCellView cellPrefab)
		{
			// see if there is a view to recycle
			var cellView = _GetRecycledCellView(cellPrefab);
			if (cellView == null)
			{
				// no recyleable cell found, so we create a new view
				// and attach it to our container
				var go = Instantiate(cellPrefab.gameObject);
				cellView = go.GetComponent<ScrollCellView>();
				cellView.transform.SetParent(content);
				cellView.transform.localPosition = Vector3.zero;
				cellView.transform.localRotation = Quaternion.identity;
			}

			return cellView;
		}

		/// <summary>
		/// This resets the internal size list and refreshes the cell views
		/// </summary>
		/// <param name="scrollPositionFactor">The percentage of the scroller to start at between 0 and 1, 0 being the start of the scroller</param>
		public void ReloadData(float scrollPositionFactor = 0)
		{
//			_reloadData = false;

			// recycle all the active cells so
			// that we are sure to get fresh views
			_RecycleAllCells();

			// if we have a delegate handling our data, then
			// call the resize
			//if (Delegate != null)
			_Resize(false);

			_scrollPosition = scrollPositionFactor * scrollSize;
			if (m_ScrollRect.vertical)
				m_ScrollRect.verticalNormalizedPosition = 1f - scrollPositionFactor;
			else
				m_ScrollRect.horizontalNormalizedPosition = scrollPositionFactor;
		}

		/// <summary>
		/// This calls the RefreshCellView method on each active cell.
		/// If you override the RefreshCellView method in your cells
		/// then you can update the UI without having to reload the data.
		/// Note: this will not change the cell sizes, you will need
		/// to call ReloadData for that to work.
		/// </summary>
		public void RefreshActiveCellViews()
		{
			for (var i = 0; i < _activeCellViews.Count; i++)
			{
				_activeCellViews[i].RefreshCellView();
			}
		}

		/// <summary>
		/// Removes all cells, both active and recycled from the scroller.
		/// This will call garbage collection.
		/// </summary>
		public void ClearAll()
		{
			ClearActive();
			ClearRecycled();
		}

		/// <summary>
		/// Removes all the active cell views. This should only be used if you want
		/// to get rid of cells because of settings set by Unity that cannot be
		/// changed at runtime. This will call garbage collection.
		/// </summary>
		public void ClearActive()
		{
			for (var i = 0; i < _activeCellViews.Count; i++)
			{
				DestroyImmediate(_activeCellViews[i].gameObject);
			}
			_activeCellViews.Clear();
		}

		/// <summary>
		/// Removes all the recycled cell views. This should only be used after you
		/// load in a completely different set of cell views that will not use the 
		/// recycled views. This will call garbage collection.
		/// </summary>
		public void ClearRecycled()
		{
			for (var i = 0; i < _recycledCellViews.Count; i++)
			{
				DestroyImmediate(_recycledCellViews[i].gameObject);
			}
			_recycledCellViews.Clear();
		}


		public void SetPositionWithDir(float value, float dir)
		{
			if (loop)
			{
				if (0 < dir && value > _loopLastJumpTrigger)
					value = _loopFirstScrollPosition + (value - _loopLastJumpTrigger);
				else if (dir < 0 && value < _loopFirstJumpTrigger)
					value = _loopLastScrollPosition - (_loopFirstJumpTrigger - value);
			}
			ScrollPosition = value;
		}


		public void JumpToDataIndex(int dataIndex,
		                            ScrollSnap.Alignment align,
		                            Method tweenType = Method.immediate,
		                            float tweenTime = 0f,
		                            Action jumpComplete = null
		)
		{
			float offset = (int)align * 0.5f;
			bool useSpacing = align == ScrollSnap.Alignment.Center;
			JumpToDataIndex(dataIndex, offset, offset, useSpacing, tweenType, tweenTime, jumpComplete);
		}

		/// <summary>
		/// Jump to a position in the scroller based on a dataIndex. This overload allows you
		/// to specify a specific offset within a cell as well.
		/// </summary>
		/// <param name="dataIndex">he data index to jump to</param>
		/// <param name="scrollerOffset">The offset from the start (top / left) of the scroller in the range 0..1.
		/// Outside this range will jump to the location before or after the scroller's viewable area</param>
		/// <param name="cellOffset">The offset from the start (top / left) of the cell in the range 0..1</param>
		/// <param name="useSpacing">Whether to calculate in the spacing of the scroller in the jump</param>
		/// <param name="tweenType">What easing to use for the jump</param>
		/// <param name="tweenTime">How long to interpolate to the jump point</param>
		/// <param name="jumpComplete">This delegate is fired when the jump completes</param>
		public void JumpToDataIndex(int dataIndex,
		                            float scrollerOffset = 0,
		                            float cellOffset = 0,
		                            bool useSpacing = true,
		                            Method tweenType = Method.immediate,
		                            float tweenTime = 0f,
		                            Action jumpComplete = null
		)
		{
			var cellOffsetPosition = 0f;

			if (cellOffset < 0 || 0 < cellOffset)
			{
				// calculate the cell offset position

				// get the cell's size
				var cellSize = GetCellViewSize(dataIndex);

				if (useSpacing)
				{
					// if using spacing add spacing from one side
					cellSize += spacing;

					// if this is not a bounday cell, then add spacing from the other side
					if (dataIndex > 0 && dataIndex < (cellCount - 1))
						cellSize += spacing;
				}

				// calculate the position based on the size of the cell and the offset within that cell
				cellOffsetPosition = cellSize * cellOffset;
			}

			var newScrollPosition = 0f;

			// cache the offset for quicker calculation
			var offset = -(scrollerOffset * scrollRectSize) + cellOffsetPosition;

			if (loop)
			{
				// if looping, then we need to determine the closest jump position.
				// we do that by checking all three sets of data locations, and returning the closest one

				// get the scroll positions for each data set.
				// Note: we are calculating the position based on the cell view index, not the data index here
				var set1Position = GetScrollPositionForCellViewIndex(dataIndex) + offset;
				var set2Position = GetScrollPositionForCellViewIndex(dataIndex + cellCount) + offset;
				var set3Position = GetScrollPositionForCellViewIndex(dataIndex + cellCount * 2) + offset;

				// get the offsets of each scroll position from the current scroll position
				var set1Diff = (Mathf.Abs(_scrollPosition - set1Position));
				var set2Diff = (Mathf.Abs(_scrollPosition - set2Position));
				var set3Diff = (Mathf.Abs(_scrollPosition - set3Position));

				// choose the smallest offset from the current position (the closest position)
				newScrollPosition = set1Diff < set2Diff
					? set1Diff < set3Diff ? set1Position : set3Position
					: set2Diff < set3Diff ? set2Position : set3Position;

//				if (set1Diff < set2Diff)
//				{
//					if (set1Diff < set3Diff)
//					{
//						newScrollPosition = set1Position;
//					}
//					else
//					{
//						newScrollPosition = set3Position;
//					}
//				}
//				else
//				{
//					if (set2Diff < set3Diff)
//					{
//						newScrollPosition = set2Position;
//					}
//					else
//					{
//						newScrollPosition = set3Position;
//					}
//				}
			}
			else
			{
				// not looping, so just get the scroll position from the dataIndex
				newScrollPosition = GetScrollPositionForDataIndex(dataIndex) + offset;
			}



			// clamp the scroll position to a valid location
//			newScrollPosition = Mathf.Clamp(newScrollPosition, 0, GetScrollPositionForCellViewIndex(_cellViewSizeArray.Count - 1, CellViewPositionEnum.Before));

			// if spacing is used, adjust the final position
			if (useSpacing)
			{
				newScrollPosition -= spacing;
				// move back by the spacing if necessary
//				newScrollPosition = Mathf.Clamp(newScrollPosition - spacing, 0, GetScrollPositionForCellViewIndex(_cellViewSizeArray.Count - 1, CellViewPositionEnum.Before));
			}

			if (scrollRect.movementType == ScrollRect.MovementType.Clamped)
			{
				newScrollPosition = Mathf.Clamp(newScrollPosition, 0, GetScrollPositionForCellViewIndex(_cellViewSizeArray.Count - 1));
			}

			m_ScrollSnap.StartSnapping(tweenType, tweenTime, ScrollPosition, newScrollPosition, jumpComplete, null);
//			scrollSnap.StartSnapping(tweenType, tweenTime, ScrollPosition, newScrollPosition, jumpComplete, null);

			// start tweening
//			m_CoTweening = StartCoroutine(TweenPosition(tweenType, tweenTime, ScrollPosition, newScrollPosition, jumpComplete));
		}

		/// <summary>
		/// Snaps the scroller on command. This is called internally when snapping is set to true and the velocity
		/// has dropped below the threshold. You can use this to manually snap whenever you like.
		/// </summary>
		public void Snap()
		{
			if (cellCount == 0)
				return;

			// スナップするインデックスを計算します.
			var snapPosition = ScrollPosition + (scrollRectSize * Mathf.Clamp01((int)m_ScrollSnap.m_Alignment * 0.5f));
			var snapCellViewIndex = _GetCellViewIndexAtPosition(snapPosition);
			var snapDataIndex = snapCellViewIndex % cellCount;

			JumpToDataIndex(snapDataIndex, m_ScrollSnap.m_Alignment, m_ScrollSnap.m_Method, m_ScrollSnap.m_Duration, null);
		}

		/// <summary>
		/// Gets the scroll position in pixels from the start of the scroller based on the cellViewIndex
		/// </summary>
		/// <param name="cellViewIndex">The cell index to look for. This is used instead of dataIndex in case of looping</param>
		/// <param name="insertPosition">Do we want the start or end of the cell view's position</param>
		/// <returns></returns>
		public float GetScrollPositionForCellViewIndex(int cellViewIndex, bool beforeCell = true)
		{
			// 要素がない場合、0を返します.
			if (cellCount == 0)
				return 0;

			// 先頭の要素を指定した場合、paddingサイズを返します.
			if (cellViewIndex == 0 && beforeCell)
				return m_ScrollRect.vertical ? padding.top : padding.left;

			// 要素数を超えていた場合、最後のセルの位置を返します.
			if (_cellViewOffsetArray.Count <= cellViewIndex)
				return _cellViewOffsetArray[_cellViewOffsetArray.Count - 2];

			if (beforeCell)
				return _cellViewOffsetArray[cellViewIndex - 1] + spacing + (m_ScrollRect.vertical ? padding.top : padding.left);
			else
				return _cellViewOffsetArray[cellViewIndex] + (m_ScrollRect.vertical ? padding.top : padding.left);
		}

		/// <summary>
		/// Gets the scroll position in pixels from the start of the scroller based on the dataIndex
		/// </summary>
		/// <param name="dataIndex">The data index to look for</param>
		/// <param name="insertPosition">Do we want the start or end of the cell view's position</param>
		/// <returns></returns>
		public float GetScrollPositionForDataIndex(int dataIndex, bool beforeCell = true)
		{
			return GetScrollPositionForCellViewIndex(loop ? cellCount + dataIndex : dataIndex, beforeCell);
		}

		/// <summary>
		/// Gets the index of a cell view at a given position
		/// </summary>
		/// <param name="position">The pixel offset from the start of the scroller</param>
		/// <returns></returns>
		int _GetCellViewIndexAtPosition(float position)
		{
			return _GetCellIndexAtPosition(position, 0, _cellViewOffsetArray.Count - 1);
		}

		#endregion

		#region Private

		/// <summary>
		/// Cached reference to the scrollRect
		/// </summary>
		private ScrollRect m_ScrollRect;

		/// <summary>
		/// Cached reference to the scrollRect's transform
		/// </summary>
		private RectTransform m_ScrollRectTransform;

		/// <summary>
		/// Flag to tell the scroller to reload the data
		/// </summary>
//		private bool _reloadData = true;

		/// <summary>
		/// Flag to tell the scroller to refresh the active list of cell views
		/// </summary>
		private bool m_NeedRefleshActive;

		/// <summary>
		/// List of views that have been recycled
		/// </summary>
		private SmallList<ScrollCellView> _recycledCellViews = new SmallList<ScrollCellView>();

		/// <summary>
		/// Cached reference to the element used to offset the first visible cell view
		/// </summary>
		private LayoutElement m_FirstPadder;

		/// <summary>
		/// Cached reference to the element used to keep the cell views at the correct size
		/// </summary>
		private LayoutElement m_LastPadder;

		/// <summary>
		/// Cached reference to the container that holds the recycled cell views
		/// </summary>
		private RectTransform _recycledCellViewContainer;

		/// <summary>
		/// Internal list of cell view sizes. This is created when the data is reloaded 
		/// to speed up processing.
		/// </summary>
		private SmallList<float> _cellViewSizeArray = new SmallList<float>();

		/// <summary>
		/// Internal list of cell view offsets. Each cell view offset is an accumulation 
		/// of the offsets previous to it.
		/// This is created when the data is reloaded to speed up processing.
		/// </summary>
		private SmallList<float> _cellViewOffsetArray = new SmallList<float>();

		/// <summary>
		/// The scrollers position
		/// </summary>
		private float _scrollPosition;

		/// <summary>
		/// The list of cell views that are currently being displayed
		/// </summary>
		private SmallList<ScrollCellView> _activeCellViews = new SmallList<ScrollCellView>();

		/// <summary>
		/// The index of the first cell view that is being displayed
		/// </summary>
		private int _activeCellViewsStartIndex;

		/// <summary>
		/// The index of the last cell view that is being displayed
		/// </summary>
		private int _activeCellViewsEndIndex;

		/// <summary>
		/// The index of the first element of the middle section of cell view sizes.
		/// Used only when looping
		/// </summary>
		private int _loopFirstCellIndex;

		/// <summary>
		/// The index of the last element of the middle seciton of cell view sizes.
		/// used only when looping
		/// </summary>
		private int _loopLastCellIndex;

		/// <summary>
		/// The scroll position of the first element of the middle seciotn of cell views.
		/// Used only when looping
		/// </summary>
		private float _loopFirstScrollPosition;

		/// <summary>
		/// The scroll position of the last element of the middle section of cell views.
		/// Used only when looping
		/// </summary>
		private float _loopLastScrollPosition;

		/// <summary>
		/// The position that triggers the scroller to jump to the end of the middle section
		/// of cell views. This keeps the scroller in the middle section as much as possible.
		/// </summary>
		private float _loopFirstJumpTrigger;

		/// <summary>
		/// The position that triggers the scroller to jump to the start of the middle section
		/// of cell views. This keeps the scroller in the middle section as much as possible.
		/// </summary>
		private float _loopLastJumpTrigger;

		/// <summary>
		/// The cached value of the last scroll rect size. This is checked every frame to see
		/// if the scroll rect has resized. If so, it will refresh.
		/// </summary>
		private float _lastScrollRectSize;

		/// <summary>
		/// The cached value of the last loop setting. This is checked every frame to see
		/// if looping was toggled. If so, it will refresh.
		/// </summary>
		private bool _lastLoop;

		/// <summary>
		/// The cell view index we are snapping to
		/// </summary>
		private int _snapCellViewIndex;

		/// <summary>
		/// The cached value of the last scrollbar visibility setting. This is checked every
		/// frame to see if the scrollbar visibility needs to be changed.
		/// </summary>
		private ScrollRect.ScrollbarVisibility _lastScrollbarVisibility;

		private float scrollSize { get { return contentSize - scrollRectSize; } }

		private float contentSize { get { return (scrollRect.vertical ? content.rect.height : content.rect.width); } }


		/// <summary>
		/// This function will create an internal list of sizes and offsets to be used in all calculations.
		/// It also sets up the loop triggers and positions and initializes the cell views.
		/// </summary>
		/// <param name="keepPosition">If true, then the scroller will try to go back to the position it was at before the resize</param>
		private void _Resize(bool keepPosition)
		{
			// cache the original position
			var originalScrollPosition = _scrollPosition;

			// clear out the list of cell view sizes and create a new list
			_cellViewSizeArray.Clear();
			var offset = _AddCellViewSizes();

			// if looping, we need to create three sets of size data
			if (loop)
			{
				// if the cells don't entirely fill up the scroll area, 
				// make some more size entries to fill it up
				if (offset < scrollRectSize)
				{
					int additionalRounds = Mathf.CeilToInt(scrollRectSize / offset);
					_DuplicateCellViewSizes(additionalRounds, _cellViewSizeArray.Count);
				}

				// set up the loop indices
				_loopFirstCellIndex = _cellViewSizeArray.Count;
				_loopLastCellIndex = _loopFirstCellIndex + _cellViewSizeArray.Count - 1;

				// create two more copies of the cell sizes
				_DuplicateCellViewSizes(2, _cellViewSizeArray.Count);
			}

			// calculate the offsets of each cell view
			_CalculateCellViewOffsets();

			// set the size of the active cell view container based on the number of cell views there are and each of their sizes
			if (m_ScrollRect.vertical)
				content.sizeDelta = new Vector2(content.sizeDelta.x, _cellViewOffsetArray.Last() + padding.top + padding.bottom);
			else
				content.sizeDelta = new Vector2(_cellViewOffsetArray.Last() + padding.left + padding.right, content.sizeDelta.y);

			// if looping, set up the loop positions and triggers
			if (loop)
			{
				_loopFirstScrollPosition = GetScrollPositionForCellViewIndex(_loopFirstCellIndex, beforeCell: true) + (spacing * 0.5f);
				_loopLastScrollPosition = GetScrollPositionForCellViewIndex(_loopLastCellIndex, beforeCell: false) - scrollRectSize + (spacing * 0.5f);

				_loopFirstJumpTrigger = _loopFirstScrollPosition - scrollRectSize;
				_loopLastJumpTrigger = _loopLastScrollPosition + scrollRectSize;
			}

			// create the visibile cells
			_ResetVisibleCellViews();

			// if we need to maintain our original position
			if (keepPosition)
				ScrollPosition = originalScrollPosition;
			else
				ScrollPosition = loop ? _loopFirstScrollPosition : 0;
//				if (loop)
//				{
//					ScrollPosition = _loopFirstScrollPosition;
//				}
//				else
//				{
//					ScrollPosition = 0;
//				}

			// set up the visibility of the scrollbar
			ScrollbarVisibility = _scrollbarVisibility;
		}

		/// <summary>
		/// Creates a list of cell view sizes for faster access
		/// </summary>
		/// <returns></returns>
		private float _AddCellViewSizes()
		{
			var offset = 0f;
			// add a size for each row in our data based on how many the delegate tells us to create
			for (var i = 0; i < cellCount; i++)
			{
				// add the size of this cell based on what the delegate tells us to use. Also add spacing if this cell isn't the first one
				_cellViewSizeArray.Add(GetCellViewSize(i) + (i == 0 ? 0 : layoutGroup.spacing));
				offset += _cellViewSizeArray[_cellViewSizeArray.Count - 1];
			}

			return offset;
		}

		/// <summary>
		/// Create a copy of the cell view sizes. This is only used in looping
		/// </summary>
		/// <param name="numberOfTimes">How many times the copy should be made</param>
		/// <param name="cellCount">How many cells to copy</param>
		private void _DuplicateCellViewSizes(int numberOfTimes, int cellCount)
		{
			for (var i = 0; i < numberOfTimes; i++)
			{
				for (var j = 0; j < cellCount; j++)
				{
					_cellViewSizeArray.Add(_cellViewSizeArray[j] + (j == 0 ? layoutGroup.spacing : 0));
				}
			}
		}

		/// <summary>
		/// Calculates the offset of each cell, accumulating the values from previous cells
		/// </summary>
		private void _CalculateCellViewOffsets()
		{
			_cellViewOffsetArray.Clear();
			var offset = 0f;
			for (var i = 0; i < _cellViewSizeArray.Count; i++)
			{
				offset += _cellViewSizeArray[i];
				_cellViewOffsetArray.Add(offset);
			}
		}

		/// <summary>
		/// Get a recycled cell with a given identifier if available
		/// </summary>
		/// <param name="cellPrefab">The prefab to check for</param>
		/// <returns></returns>
		private ScrollCellView _GetRecycledCellView(ScrollCellView cellPrefab)
		{
			for (var i = 0; i < _recycledCellViews.Count; i++)
			{
				if (_recycledCellViews[i].cellIdentifier == cellPrefab.cellIdentifier)
				{
					// the cell view was found, so we use this recycled one.
					// we also remove it from the recycled list
					var cellView = _recycledCellViews.RemoveAt(i);
					return cellView;
				}
			}

			return null;
		}

		/// <summary>
		/// This sets up the visible cells, adding and recycling as necessary
		/// </summary>
		private void _ResetVisibleCellViews()
		{
			int startIndex;
			int endIndex;

			// calculate the range of the visible cells
			_CalculateCurrentActiveCellRange(out startIndex, out endIndex);

			// go through each previous active cell and recycle it if it no longer falls in the range
			var i = 0;
			SmallList<int> remainingCellIndices = new SmallList<int>();
			while (i < _activeCellViews.Count)
			{
				if (_activeCellViews[i].cellIndex < startIndex || _activeCellViews[i].cellIndex > endIndex)
				{
					_RecycleCell(_activeCellViews[i]);
				}
				else
				{
					// this cell index falls in the new range, so we add its
					// index to the reusable list
					remainingCellIndices.Add(_activeCellViews[i].cellIndex);
					i++;
				}
			}

			if (remainingCellIndices.Count == 0)
			{
				// there were no previous active cells remaining, 
				// this list is either brand new, or we jumped to 
				// an entirely different part of the list.
				// just add all the new cell views

				for (i = startIndex; i <= endIndex; i++)
				{
					_AddCellView(i, false);
				}
			}
			else
			{
				// we are able to reuse some of the previous
				// cell views

				// first add the views that come before the 
				// previous list, going backward so that the
				// new views get added to the front
				for (i = endIndex; i >= startIndex; i--)
				{
					if (i < remainingCellIndices.First())
					{
						_AddCellView(i, true);
					}
				}

				// next add teh views that come after the
				// previous list, going forward and adding
				// at the end of the list
				for (i = startIndex; i <= endIndex; i++)
				{
					if (i > remainingCellIndices.Last())
					{
						_AddCellView(i, false);
					}
				}
			}

			// update the start and end indices
			_activeCellViewsStartIndex = startIndex;
			_activeCellViewsEndIndex = endIndex;

			// adjust the padding elements to offset the cell views correctly
			_SetPadders();
		}

		/// <summary>
		/// Recycles all the active cells
		/// </summary>
		private void _RecycleAllCells()
		{
			while (_activeCellViews.Count > 0)
				_RecycleCell(_activeCellViews[0]);
			_activeCellViewsStartIndex = 0;
			_activeCellViewsEndIndex = 0;
		}

		/// <summary>
		/// Recycles one cell view
		/// </summary>
		/// <param name="cellView"></param>
		private void _RecycleCell(ScrollCellView cellView)
		{
			OnWillRecycleCellView(cellView);

			// remove the cell view from the active list
			_activeCellViews.Remove(cellView);

			// add the cell view to the recycled list
			_recycledCellViews.Add(cellView);

			// move the GameObject to the recycled container
			cellView.transform.SetParent(_recycledCellViewContainer);

			// reset the cellView's properties
			cellView.dataIndex = 0;
			cellView.cellIndex = 0;
			cellView.active = false;

			OnChangedCellViewVisibility(cellView);
		}

		/// <summary>
		/// Creates a cell view, or recycles if it can
		/// </summary>
		/// <param name="cellIndex">The index of the cell view</param>
		/// <param name="listPosition">Whether to add the cell to the beginning or the end</param>
		private void _AddCellView(int cellIndex, bool atStart)
		{
			if (cellCount == 0)
				return;

			// get the dataIndex. Modulus is used in case of looping so that the first set of cells are ignored
			var dataIndex = cellIndex % cellCount;
			// request a cell view from the delegate
			var cellView = GetCellView(dataIndex, cellIndex);

			// set the cell's properties
			cellView.cellIndex = cellIndex;
			cellView.dataIndex = dataIndex;
			cellView.active = true;

			// add the cell view to the active container
			cellView.transform.SetParent(content, false);
			cellView.transform.localScale = Vector3.one;

			// add a layout element to the cellView
			LayoutElement layoutElement = cellView.GetComponent<LayoutElement>();
			if (layoutElement == null)
				layoutElement = cellView.gameObject.AddComponent<LayoutElement>();

			// set the size of the layout element
			if (m_ScrollRect.vertical)
				layoutElement.minHeight = _cellViewSizeArray[cellIndex] - (cellIndex > 0 ? layoutGroup.spacing : 0);
			else
				layoutElement.minWidth = _cellViewSizeArray[cellIndex] - (cellIndex > 0 ? layoutGroup.spacing : 0);

			// add the cell to the active list
			if (atStart)
				_activeCellViews.AddStart(cellView);
			else
				_activeCellViews.Add(cellView);

			// set the hierarchy position of the cell view in the container
			cellView.transform.SetSiblingIndex(atStart ? 1 : content.childCount - 2);

			OnChangedCellViewVisibility(cellView);
		}

		/// <summary>
		/// This function adjusts the two padders that control the first cell view's
		/// offset and the overall size of each cell.
		/// </summary>
		private void _SetPadders()
		{
			if (cellCount == 0)
				return;

			// calculate the size of each padder
			var firstSize = _cellViewOffsetArray[_activeCellViewsStartIndex] - _cellViewSizeArray[_activeCellViewsStartIndex];
			var lastSize = _cellViewOffsetArray.Last() - _cellViewOffsetArray[_activeCellViewsEndIndex];

			if (m_ScrollRect.vertical)
			{
				// set the first padder and toggle its visibility
				m_FirstPadder.minHeight = firstSize;
				m_FirstPadder.gameObject.SetActive(m_FirstPadder.minHeight > 0);

				// set the last padder and toggle its visibility
				m_LastPadder.minHeight = lastSize;
				m_LastPadder.gameObject.SetActive(m_LastPadder.minHeight > 0);
			}
			else
			{
				// set the first padder and toggle its visibility
				m_FirstPadder.minWidth = firstSize;
				m_FirstPadder.gameObject.SetActive(m_FirstPadder.minWidth > 0);

				// set the last padder and toggle its visibility
				m_LastPadder.minWidth = lastSize;
				m_LastPadder.gameObject.SetActive(m_LastPadder.minWidth > 0);
			}
		}

		/// <summary>
		/// This function is called if the scroller is scrolled, updating the active list of cells
		/// </summary>
		private void _RefreshActive()
		{
			m_NeedRefleshActive = false;

			int startIndex;
			int endIndex;
			var nextVelocity = Vector2.zero;

			// if looping, check to see if we scrolled past a trigger
			if (loop)
			{
				if (_scrollPosition < _loopFirstJumpTrigger)
				{
					nextVelocity = m_ScrollRect.velocity;
					ScrollPosition = _loopLastScrollPosition - (_loopFirstJumpTrigger - _scrollPosition);
					m_ScrollRect.velocity = nextVelocity;
				}
				else if (_scrollPosition > _loopLastJumpTrigger)
				{
					nextVelocity = m_ScrollRect.velocity;
					ScrollPosition = _loopFirstScrollPosition + (_scrollPosition - _loopLastJumpTrigger);
					m_ScrollRect.velocity = nextVelocity;
				}
			}

			// get the range of visibile cells
			_CalculateCurrentActiveCellRange(out startIndex, out endIndex);

			// if the index hasn't changed, ignore and return
			if (startIndex == _activeCellViewsStartIndex && endIndex == _activeCellViewsEndIndex)
				return;

			// recreate the visibile cells
			_ResetVisibleCellViews();
		}

		/// <summary>
		/// Determines which cells can be seen
		/// </summary>
		/// <param name="startIndex">The index of the first cell visible</param>
		/// <param name="endIndex">The index of the last cell visible</param>
		private void _CalculateCurrentActiveCellRange(out int startIndex, out int endIndex)
		{
			startIndex = 0;
			endIndex = 0;

			// get the positions of the scroller
			var startPosition = _scrollPosition;
			var endPosition = _scrollPosition + scrollRectSize;//(m_ScrollRect.vertical ? m_ScrollRectTransform.rect.height : m_ScrollRectTransform.rect.width);

			// calculate each index based on the positions
			startIndex = _GetCellViewIndexAtPosition(startPosition);
			endIndex = _GetCellViewIndexAtPosition(endPosition);
		}

		/// <summary>
		/// Gets the index of a cell at a given position based on a subset range.
		/// This function uses a recursive binary sort to find the index faster.
		/// </summary>
		/// <param name="position">The pixel offset from the start of the scroller</param>
		/// <param name="startIndex">The first index of the range</param>
		/// <param name="endIndex">The last index of the rnage</param>
		/// <returns></returns>
		private int _GetCellIndexAtPosition(float position, int startIndex, int endIndex)
		{
			// if the range is invalid, then we found our index, return the start index
			if (startIndex >= endIndex)
				return startIndex;

			// determine the middle point of our binary search
			var middleIndex = (startIndex + endIndex) / 2;

			// if the middle index is greater than the position, then search the last
			// half of the binary tree, else search the first half
			if ((_cellViewOffsetArray[middleIndex] + (m_ScrollRect.vertical ? padding.top : padding.left)) >= position)
				return _GetCellIndexAtPosition(position, startIndex, middleIndex);
			else
				return _GetCellIndexAtPosition(position, middleIndex + 1, endIndex);
		}

		public RectTransform content
		{
			get
			{
				if (!m_Content)
				{
					var sr = scrollRect;
					if (!sr.content)
					{
						var go = new GameObject("Content", typeof(RectTransform));
						go.transform.SetParent(sr.transform);
						sr.content = go.GetComponent<RectTransform>();
					}
					m_Content = scrollRect.content;
				}
				return m_Content;
			}
		}

		RectTransform m_Content;


		public ScrollRect scrollRect
		{
			get
			{
				if (!m_ScrollRect)
					m_ScrollRect = GetComponent<ScrollRect>();
				return m_ScrollRect;
			}
		}

		public HorizontalOrVerticalLayoutGroup layoutGroup
		{
			get
			{
				if (!m_LayoutGroup)
					m_LayoutGroup = content.GetComponent<HorizontalOrVerticalLayoutGroup>();

				if (!m_LayoutGroup || scrollRect.vertical != (m_LayoutGroup is VerticalLayoutGroup))
				{
#if UNITY_EDITOR
					if (!Application.isPlaying)
						UnityEditor.EditorApplication.delayCall += UpdateLauyout;
					else
#endif
						UpdateLauyout();
				}

				return m_LayoutGroup;
			}
		}

		HorizontalOrVerticalLayoutGroup m_LayoutGroup;


		void UpdateLauyout()
		{
			var type = scrollRect.vertical ? typeof(VerticalLayoutGroup) : typeof(HorizontalLayoutGroup);
			var layout = content.GetComponent<LayoutGroup>();
			if (!layout)
			{
				m_LayoutGroup = content.gameObject.AddComponent(type) as HorizontalOrVerticalLayoutGroup;
			}
			else if (scrollRect.vertical != (layout is VerticalLayoutGroup))
			{
#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					ComponentConverter.ConvertTo(layout, type);
					m_LayoutGroup = content.GetComponent<HorizontalOrVerticalLayoutGroup>();
				}
				else
#endif
				{
					RectOffset pad = layout ? layout.padding : new RectOffset();
					float space = (layout is HorizontalOrVerticalLayoutGroup) ? (layout as HorizontalOrVerticalLayoutGroup).spacing : 0;
					DestroyImmediate(layout);
					m_LayoutGroup = content.gameObject.AddComponent(type) as HorizontalOrVerticalLayoutGroup;
					m_LayoutGroup.padding = pad;
					m_LayoutGroup.spacing = space;
				}
			}
		}

		public Scrollbar scrollbar { get { return scrollRect.vertical ? scrollRect.verticalScrollbar : scrollRect.horizontalScrollbar; } }

		//==== v MonoBehavior Callbacks v ====
		/// <summary>
		/// Caches and initializes the scroller
		/// </summary>
		protected virtual void Awake()
		{
//			GameObject go;

			// cache some components
//			m_ScrollRect = this.GetComponent<ScrollRect>();
			m_ScrollRectTransform = scrollRect.transform as RectTransform;
			m_ScrollSnap.Initialize(this);

			if (!scrollRect.vertical && !scrollRect.horizontal)
			{
				//無効なスクロール
				enabled = false;
				return;
			}

			// force the scroller to scroll in the direction we want
			// set the containers anchor and pivot
			if (scrollRect.vertical)
			{
				scrollRect.horizontal = false;
				content.anchorMin = new Vector2(0, 1);
				content.anchorMax = Vector2.one;
				content.pivot = new Vector2(0.5f, 1f);
				_scrollbarVisibility = scrollRect.verticalScrollbarVisibility;
			}
			else
			{
				scrollRect.vertical = false;
				content.anchorMin = Vector2.zero;
				content.anchorMax = new Vector2(0, 1f);
				content.pivot = new Vector2(0, 0.5f);
				_scrollbarVisibility = scrollRect.horizontalScrollbarVisibility;
			}

			content.offsetMax = Vector2.zero;
			content.offsetMin = Vector2.zero;
			content.localPosition = Vector3.zero;
			content.localRotation = Quaternion.identity;
			content.localScale = Vector3.one;

			layoutGroup.childAlignment = TextAnchor.UpperLeft;
			layoutGroup.childForceExpandHeight = true;
			layoutGroup.childForceExpandWidth = true;

			// create the padder objects
			GameObject go = new GameObject("First Padder", typeof(RectTransform), typeof(LayoutElement));
			go.transform.SetParent(content, false);
			m_FirstPadder = go.GetComponent<LayoutElement>();

			go = new GameObject("Last Padder", typeof(RectTransform), typeof(LayoutElement));
			go.transform.SetParent(content, false);
			m_LastPadder = go.GetComponent<LayoutElement>();

			// create the recycled cell view container
			go = new GameObject("Recycled Cells", typeof(RectTransform));
			go.transform.SetParent(m_ScrollRect.transform, false);
			_recycledCellViewContainer = go.GetComponent<RectTransform>();
			_recycledCellViewContainer.gameObject.SetActive(false);

			// set up the last values for updates
			_lastScrollRectSize = scrollRectSize;
			_lastLoop = loop;
			_lastScrollbarVisibility = _scrollbarVisibility;

			// スクロール値が変化した時、コールバックを受け取ります.
			scrollRect.onValueChanged.AddListener(val =>
				{
					ScrollPosition = scrollRect.vertical
						? (1f - val.y) * scrollSize
						: val.x * scrollSize;
					_RefreshActive();
				});
		}

//		protected virtual void Start()
//		{
//			ReloadData();
//		}

		/// <summary>
		/// LateUpdate is called every frame, if the Behaviour is enabled.
		/// </summary>
		protected virtual void LateUpdate()
		{
			m_ScrollSnap.Update();
//			if (_reloadData)
//			{
//				// if the reload flag is true, then reload the data
//				ReloadData();
//			}

			// if the scroll rect size has changed and looping is on,
			// or the loop setting has changed, then we need to resize
			if ((loop && _lastScrollRectSize != scrollRectSize) || (loop != _lastLoop))
			{
				_Resize(true);
				_lastScrollRectSize = scrollRectSize;

				_lastLoop = loop;
			}

			// update the scroll bar visibility if it has changed
			if (_lastScrollbarVisibility != _scrollbarVisibility)
			{
				ScrollbarVisibility = _scrollbarVisibility;
				_lastScrollbarVisibility = _scrollbarVisibility;
			}

			if (m_NeedRefleshActive)
			{
				_RefreshActive();
			}
		}
		//==== ^ MonoBehavior Callbacks ^ ====

		/*
		/// <summary>
		/// This function is called when the object becomes enabled and active.
		/// </summary>
		protected virtual void OnEnable()
		{
			m_ScrollRect.onValueChanged.AddListener(ScrollRect_OnValueChanged);
		}

		/// <summary>
		/// This function is called when the behaviour becomes disabled () or inactive.
		/// </summary>
		protected virtual void OnDisable()
		{
			m_ScrollRect.onValueChanged.RemoveListener(ScrollRect_OnValueChanged);
		}

		/// <summary>
		/// Handler for when the scroller changes value
		/// </summary>
		/// <param name="val">The scroll rect's value</param>
		private void ScrollRect_OnValueChanged(Vector2 val)
		{
			if (!enabled)
				return;
			
			ScrollPosition = scrollRect.vertical
				?  (1f - val.y) * scrollSize
				: val.x * scrollSize;
			
			_RefreshActive();
					
//			// set the internal scroll position
//			if (m_ScrollRect.vertical)
//				ScrollPosition = (1f - val.y) * scrollSize;
//			else
//				ScrollPosition = val.x * scrollSize;
//
//			_RefreshActive();
		}
		*/


		//#### v IScrollSnap Implementation v ####
		/// <summary>
		/// Called by the EventSystem when a Scroll event occurs.
		/// </summary>
		public virtual void OnScroll(PointerEventData eventData)
		{
			m_ScrollSnap.OnScroll();
		}

		/// <summary>
		/// Called before a drag is started.
		/// </summary>
		public virtual void OnBeginDrag(PointerEventData eventData)
		{
			m_ScrollSnap.OnBeginDrag();
		}

		/// <summary>
		/// Called by the EventSystem once dragging ends.
		/// </summary>
		public virtual void OnEndDrag(PointerEventData eventData)
		{
			m_ScrollSnap.OnEndDrag();
		}
		//#### ^ IScrollSnap Implementation ^ ####

		#endregion
	}
}