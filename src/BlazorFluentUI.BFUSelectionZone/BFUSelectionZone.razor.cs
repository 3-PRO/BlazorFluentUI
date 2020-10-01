﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace BlazorFluentUI
{
    public partial class BFUSelectionZone<TItem> : BFUComponentBase, IAsyncDisposable
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public bool DisableAutoSelectOnInputElements { get; set; }

        [Parameter]
        public bool DisableRenderOnSelectionChanged { get; set; } = false;

        [Parameter]
        public bool EnableTouchInvocationTarget { get; set; } = false;

        [Parameter]
        public bool EnterModalOnTouch { get; set; }

        [Parameter]
        public bool IsSelectedOnFocus { get; set; } = true;

        [Parameter]
        public Action<TItem, int> OnItemContextMenu { get; set; }

        [Parameter]
        public Action<TItem, int> OnItemInvoked { get; set; }

        private Selection<TItem> _selection;
        [Parameter]
        public Selection<TItem> Selection 
        { 
            get => _selection; 
            set 
            {
                if (_selection != value)
                {
                    if (_selection != null && _selectionSubscription != null)
                    {
                        _selectionSubscription.Dispose();
                    }
                    _selection = value;
                    if (_selection != null)
                    {
                        _selectionSubscription = _selection.SelectionChanged.Subscribe(_ => { });//InvokeAsync(StateHasChanged));
                    }
                }
            } 
        }

        IDisposable _selectionSubscription;
        //[Parameter]
        //public EventCallback<Selection<TItem>> SelectionChanged { get; set; }

        [Parameter]
        public SelectionMode SelectionMode { get; set; }

        [Parameter]
        public bool SelectionPreservedOnEmptyClick { get; set; }

        [Inject]
        private IJSRuntime? JSRuntime { get; set; }

        private bool isModal = false;


        //private HashSet<TItem> selectedItems = new HashSet<TItem>();

        //private BehaviorSubject<ICollection<TItem>> selectedItemsSubject;

        //public IObservable<ICollection<TItem>> SelectedItemsObservable { get; private set; }

        private bool doNotRenderOnce = false;

        private DotNetObjectReference<BFUSelectionZone<TItem>>? dotNetRef;
        private BFUSelectionZoneProps props;

        protected override bool ShouldRender()
        {
            if (doNotRenderOnce && DisableRenderOnSelectionChanged)
            {
                doNotRenderOnce = false;
                return false;
            }
            else
                doNotRenderOnce = false;

            return true;
            //return base.ShouldRender();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (props == null)
            {
                props = GenerateProps();
            }

            if (dotNetRef != null)
            {
                if (isModal != props!.IsModal
                    || SelectionMode != props.SelectionMode
                    || DisableAutoSelectOnInputElements != props.DisableAutoSelectOnInputElements
                    || EnterModalOnTouch != props.EnterModalOnTouch
                    || EnableTouchInvocationTarget != props.EnableTouchInvocationTarget
                    || (OnItemInvoked!= null) != props.OnItemInvokeSet)
                {
                    props = GenerateProps();
                    await JSRuntime!.InvokeVoidAsync("BlazorFluentUiSelectionZone.updateProps", dotNetRef, props);
                }
            }

            await base.OnParametersSetAsync();
        }

        private BFUSelectionZoneProps GenerateProps()
        {
            return new BFUSelectionZoneProps
            {
                IsModal = isModal,
                SelectionMode = this.SelectionMode,
                DisableAutoSelectOnInputElements = this.DisableAutoSelectOnInputElements,
                EnterModalOnTouch = this.EnterModalOnTouch,
                EnableTouchInvocationTarget=this.EnableTouchInvocationTarget,
                OnItemInvokeSet = (OnItemInvoked != null)
            };
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                dotNetRef = DotNetObjectReference.Create(this);
                await JSRuntime!.InvokeVoidAsync("BlazorFluentUiSelectionZone.registerSelectionZone", dotNetRef, RootElementReference, new BFUSelectionZoneProps { IsModal = this.isModal, SelectionMode = this.SelectionMode });
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        public async ValueTask DisposeAsync()
        {
            if (dotNetRef != null)
            {
                await JSRuntime!.InvokeVoidAsync("BlazorFluentUiSelectionZone.unregisterSelectionZone", dotNetRef);
                dotNetRef.Dispose();
            }
        }

        [JSInvokable]
        public int GetItemsLength()
        {
            return Selection.GetItems().Count;
        }

        [JSInvokable]
        public bool IsIndexSelected(int index)
        {
            return Selection.IsIndexSelected(index);
        }

        [JSInvokable]
        public int GetSelectedCount()
        {
            var count = Selection.GetSelectedCount();
            if (count.HasValue)
                return count.Value;
            else
                return 0;
        }

        //[JSInvokable]
        //public void ClearAndSelectIndex(int index)
        //{
        //    Selection.SetChangeEvents(false);
        //    Selection.SetAllSelected(false);
        //    Selection.SetIndexSelected(index, true, true);
        //}

        [JSInvokable]
        public void SetModal(bool isModal)
        {
            Selection.SetModal(isModal);
        }

        [JSInvokable]
        public void SetChangeEvents(bool change)
        {
            Selection.SetChangeEvents(change);
        }

        [JSInvokable]
        public void ToggleAllSelected()
        {
            Selection.ToggleAllSelected();
        }

        [JSInvokable]
        public void ToggleIndexSelected(int index)
        {
            Selection.ToggleIndexSelected(index);
            //StateHasChanged();
        }

        [JSInvokable]
        public void SetAllSelected(bool isAllSelected)
        {
            Selection.SetAllSelected(isAllSelected);
            //InvokeAsync(StateHasChanged);
        }

        [JSInvokable]
        public void SetIndexSelected(int index, bool isSelected, bool shouldAnchor)
        {
            Selection.SetIndexSelected(index, isSelected, shouldAnchor);
            //StateHasChanged();
        }

        [JSInvokable]
        public void SelectToIndex(int index, bool clearSelection)
        {
            Selection.SelectToIndex(index,clearSelection);
        }

        [JSInvokable]
        public void InvokeItem(int index)
        {
            OnItemInvoked?.Invoke(Selection.GetItems()[index], index);
        }

    }
}
