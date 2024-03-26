using System;
//using Microsoft.Maui.Controls.Compatibility;

namespace EwDavidForms
{
    /// <summary>
    ///     This behavior allows you to automatically insert a UI element between children of a layout such as stacklayout.
    ///     The simplest usage of this is adding a divider line between each UI element in a stacklayout.
    ///     By default, this will add a BoxView with a HeightRequest of 1 and a BackgroundColor set to <see cref="ColorPalette.GrayDividerLine"/>
    ///         You can override the UI element by passing your own <see cref="DataTemplate"/> in the <see cref="ItemTemplate"/> property.
    ///         
    ///     How It works:
    ///         > We manage the elements through the ChildAdded and ChildRemoved events of the layout.
    ///         > When Adding, we always skip the first element (index 0) and for the other elements, we insert the dynamic view (<see cref="ItemTemplate"/>) before the actual added child
    ///             * The visibility of the Dynamic view is dependent on the visibility of the the associated child
    ///             * We mark all Dynamic View with the <see cref="IsDynamicallyGeneratedProperty"/> bindable property.
    ///             * We attach the generated dynamic view to the child through <see cref="AttachedDynamicViewProperty"/> bindable property.
    ///             * Example:
    ///                 ~ [0] - Child 1 -> Index will remain as 0
    ///                 ~ Insert Dynamic View Here -> Index will be 1 and will be associated to Child 2 though bindable properties.
    ///                 ~ [1] - Child 2 -> Index will be 2
    ///         > When Removing, we check if there is an attached dyamic view in the removed child by getting the <see cref="AttachedDynamicViewProperty"/>
    ///             * If we do get a value, we just ensure that it's removed in the child list of the layout and clear up any referencing bindable property (like <see cref="AttachedDynamicViewProperty"/>)
    ///         > Lastly, we have the <see cref="RefreshDynamicViews(bool)"/> method to recreate everything.
    ///             * This happens when:
    ///                 ~ The value of <see cref="ItemTemplate"/> property changes during runtime, 
    ///                 ~ The behavior is being attached or detached
    ///             * This method is the most costly among the others methods in this class, only run this if you must such as when the ItemTemplate value changes.
    /// </summary>
    /// <remarks>
    ///     The private bindable properties here are intentional.
    ///     It is the way this behavior manages and keep tracks of things.
    ///     DO NOT, in any way, make <see cref="IsDynamicallyGeneratedProperty"/> and <see cref="AttachedDynamicViewProperty"/>
    ///     PUBLIC as well as the associated methods to it. If you must, be sure that you have thoroughly understood how it's being used and why it's used that way.
    ///     
    ///     In the event that you have conflicts between private bindable properties, then the most simplest solution is to ensure that they have different underlying names.
    /// </remarks>
    //public class DynamicViewBetweenChildrenBehavior : BehaviorBase<Microsoft.Maui.Controls.Compatibility.Layout<View>>
    public class DynamicViewBetweenChildrenBehavior : BehaviorBase<GenericLayout<View>>
    {
        private static readonly BindableProperty IsDynamicallyGeneratedProperty = BindableProperty.CreateAttached(
            "IsDynamicallyGenerated",
            typeof(bool),
            typeof(DynamicViewBetweenChildrenBehavior),
            defaultValue: false);

        private static readonly BindableProperty AttachedDynamicViewProperty = BindableProperty.CreateAttached(
            "AttachedDynamicView",
            typeof(View),
            typeof(DynamicViewBetweenChildrenBehavior),
            defaultValue: null);

        private static bool GetIsDynamicallyGenerated(View view)
        {
            return (bool)view.GetValue(IsDynamicallyGeneratedProperty);
        }

        private static void SetIsDynamicallyGenerated(View view, bool value)
        {
            view.SetValue(IsDynamicallyGeneratedProperty, value);
        }

        private static View GetAttachedDynamicView(View view)
        {
            return (View)view.GetValue(AttachedDynamicViewProperty);
        }

        private static void SetAttachedDynamicView(View view, View value)
        {
            view.SetValue(AttachedDynamicViewProperty, value);
        }

        public static readonly BindableProperty ItemTemplateProperty =
            BindableProperty.Create(
                nameof(ItemTemplate),
                typeof(DataTemplate),
                typeof(DynamicViewBetweenChildrenBehavior),
                propertyChanged: (bindable, _, __) => ((DynamicViewBetweenChildrenBehavior)bindable).HandleOnItemTemplateChanged(),
                defaultValueCreator: _ => new DataTemplate(
                    () => new BoxView
                    {
                        HeightRequest = 1,
                        BackgroundColor = Colors.DarkGray
                    }));

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        //protected override void OnAttachedTo(Layout<View> bindable)
        protected override void OnAttachedTo(GenericLayout<View> bindable)
        {
            base.OnAttachedTo(bindable);

            RefreshDynamicViews(true);
            AddEventHandlers();
        }

        //protected override void OnDetachingFrom(Layout<View> bindable)
        protected override void OnDetachingFrom(GenericLayout<View> bindable)
        {
            RemoveEventHandlers();
            RefreshDynamicViews(false);

            base.OnDetachingFrom(bindable);
        }

        private void AssociatedObject_ChildAdded(object sender, ElementEventArgs e)
        {
            if (AssociatedObject?.Children == null || e?.Element == null || !(e.Element is View view) || view == null)
            {
                return;
            }

            // It is neccessary to invoke it on the main thread.
            // This queues up the work on the main thread and breaks it off from running during the event cycle of the
            // observable collection implementation of the ChildAddedd event
            // If you remove this, it will cause a reentrancy error in the underlying ObservableCollection implementation.
            Device.BeginInvokeOnMainThread(() => HandleChildAdded(view));
        }

        private void AssociatedObject_ChildRemoved(object sender, ElementEventArgs e)
        {
            if (AssociatedObject?.Children == null || e?.Element == null || !(e.Element is View view) || view == null)
            {
                return;
            }

            // It is neccessary to invoke it on the main thread.
            // This queues up the work on the main thread and breaks it off from running during the event cycle of the
            // observable collection implementation of the ChildRemoved event.
            // If you remove this, it will cause a reentrancy error in the underlying ObservableCollection implementation.
            Device.BeginInvokeOnMainThread(() => HandleChildRemoved(view));
        }

        private void HandleChildAdded(View view)
        {
            if (view == null || AssociatedObject?.Children == null)
            {
                return;
            }

            var isDynamicallyGenerated = GetIsDynamicallyGenerated(view);

            if (!isDynamicallyGenerated)
            {
                var index = AssociatedObject.Children.IndexOf(view);

                if (index > 0)
                {
                    var dynamicView = CreateDynamicViewFor(view);

                    if (dynamicView != null)
                    {
                        AssociatedObject.Children.Insert(index, dynamicView);
                    }
                }
            }
        }

        private void HandleChildRemoved(View view)
        {
            if (AssociatedObject?.Children == null || view == null)
            {
                return;
            }

            var isDynamicallyGenerated = GetIsDynamicallyGenerated(view);

            if (!isDynamicallyGenerated)
            {
                var attachedDynamicView = GetAttachedDynamicView(view);

                if (attachedDynamicView != null)
                {
                    // Remove any lingering attachments.
                    SetAttachedDynamicView(view, null);

                    if (AssociatedObject.Children.Contains(attachedDynamicView))
                    {
                        AssociatedObject.Children.Remove(attachedDynamicView);
                    }
                }
            }
        }

        private void HandleOnItemTemplateChanged()
        {
            RemoveEventHandlers();
            RefreshDynamicViews(AssociatedObject != null);
            AddEventHandlers();
        }

        private void RefreshDynamicViews(bool isAttached)
        {
            if (AssociatedObject?.Children?.Any() != true)
            {
                return;
            }

            //var actualChildren = AssociatedObject.Children.Where(x => x != null && !GetIsDynamicallyGenerated(x)).ToList();
            var actualChildren = AssociatedObject.Children.Where(x => x != null && !GetIsDynamicallyGenerated((View)x)).ToList();

            AssociatedObject.Children.Clear();

            for (int i = 0; i < actualChildren.Count(); i++)
            {
                var actualChild = actualChildren[i];

                if (actualChild == null)
                {
                    continue;
                }

                // Remove any lingering attachments.
                //SetAttachedDynamicView(actualChild, null);
                SetAttachedDynamicView((View)actualChild, null);

                // This behavior must be attached.
                // and we should not be the first element
                if (isAttached && i != 0)
                {
                    //var dynamicView = CreateDynamicViewFor(actualChild);
                    var dynamicView = CreateDynamicViewFor((View)actualChild);

                    if (dynamicView != null)
                    {
                        AssociatedObject.Children.Add(dynamicView);
                    }
                }

                AssociatedObject.Children.Add(actualChild);
            }
        }

        private View CreateDynamicViewFor(View element)
        {
            var dynamicView = ItemTemplate?.CreateContent() as View;

            if (dynamicView != null)
            {
                SetIsDynamicallyGenerated(dynamicView, true);
                if (element != null)
                {
                    dynamicView.SetBinding(View.IsVisibleProperty, new Binding(View.IsVisibleProperty.PropertyName, source: element));
                    SetAttachedDynamicView(element, dynamicView);
                }
            }

            return dynamicView;
        }

        private void AddEventHandlers()
        {
            if (AssociatedObject == null)
            {
                return;
            }

            AssociatedObject.ChildAdded += AssociatedObject_ChildAdded;
            AssociatedObject.ChildRemoved += AssociatedObject_ChildRemoved;
        }

        private void RemoveEventHandlers()
        {
            if (AssociatedObject == null)
            {
                return;
            }

            AssociatedObject.ChildAdded -= AssociatedObject_ChildAdded;
            AssociatedObject.ChildRemoved -= AssociatedObject_ChildRemoved;
        }
    }
}
