﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Reactive;
using DynamicData.Binding;
using Xunit;

namespace ReactiveUI.Tests
{
    public class PropertyBindModel
    {
        public int AThing { get; set; }

        public string AnotherThing { get; set; }
    }

    public class PropertyBindViewModel : ReactiveObject
    {
        public string _Property1;

        public PropertyBindModel _Model;

        public int _Property2;

        public double _JustADouble;

        public decimal _JustADecimal;

        public double? _NullableDouble;

        public int _JustAInt32;

        public PropertyBindViewModel(PropertyBindModel model = null)
        {
            Model = model ?? new PropertyBindModel { AThing = 42, AnotherThing = "Baz" };
            SomeCollectionOfStrings = new ObservableCollectionExtended<string>(new[] { "Foo", "Bar" });
        }

        public string Property1
        {
            get => _Property1;
            set => this.RaiseAndSetIfChanged(ref _Property1, value);
        }

        public int Property2
        {
            get => _Property2;
            set => this.RaiseAndSetIfChanged(ref _Property2, value);
        }

        public double JustADouble
        {
            get => _JustADouble;
            set => this.RaiseAndSetIfChanged(ref _JustADouble, value);
        }

        public decimal JustADecimal
        {
            get => _JustADecimal;
            set => this.RaiseAndSetIfChanged(ref _JustADecimal, value);
        }

        public int JustAInt32
        {
            get => _JustAInt32;
            set => this.RaiseAndSetIfChanged(ref _JustAInt32, value);
        }

        public double? NullableDouble
        {
            get => _NullableDouble;
            set => this.RaiseAndSetIfChanged(ref _NullableDouble, value);
        }

        public ObservableCollectionExtended<string> SomeCollectionOfStrings { get; }

        public PropertyBindModel Model
        {
            get => _Model;
            set => this.RaiseAndSetIfChanged(ref _Model, value);
        }
    }

    public class PropertyBindingTest
    {
        [WpfFact]
        [UseInvariantCulture]
        public void TwoWayBindWithFuncConvertersSmokeTest()
        {
            var vm = new PropertyBindViewModel();
            var view = new PropertyBindView { ViewModel = vm };
            var fixture = new PropertyBinderImplementation();

            vm.JustADecimal = 123.45m;
            Assert.NotEqual(vm.JustADecimal.ToString(), view.SomeTextBox.Text);

            var disp = fixture.Bind(vm, view, x => x.JustADecimal, x => x.SomeTextBox.Text, (IObservable<Unit>)null, d => d.ToString(), decimal.Parse);

            Assert.Equal(vm.JustADecimal.ToString(), view.SomeTextBox.Text);
            Assert.Equal(123.45m, vm.JustADecimal);

            view.SomeTextBox.Text = "567.89";
            Assert.Equal(567.89m, vm.JustADecimal);

            disp.Dispose();
            vm.JustADecimal = 0;

            Assert.Equal(0, vm.JustADecimal);
            Assert.Equal("567.89", view.SomeTextBox.Text);
        }

        [WpfFact]
        public void TwoWayBindSmokeTest()
        {
            var vm = new PropertyBindViewModel();
            var view = new PropertyBindView { ViewModel = vm };
            var fixture = new PropertyBinderImplementation();

            vm.Property1 = "Foo";
            Assert.NotEqual(vm.Property1, view.SomeTextBox.Text);

            var disp = fixture.Bind(vm, view, x => x.Property1, x => x.SomeTextBox.Text, (IObservable<Unit>)null, null);

            Assert.Equal(vm.Property1, view.SomeTextBox.Text);
            Assert.Equal("Foo", vm.Property1);

            view.SomeTextBox.Text = "Bar";
            Assert.Equal(vm.Property1, "Bar");

            disp.Dispose();
            vm.Property1 = "Baz";

            Assert.Equal("Baz", vm.Property1);
            Assert.NotEqual(vm.Property1, view.SomeTextBox.Text);
        }

        [WpfFact]
        public void TypeConvertedTwoWayBindSmokeTest()
        {
            var vm = new PropertyBindViewModel();
            var view = new PropertyBindView { ViewModel = vm };
            var fixture = new PropertyBinderImplementation();

            vm.Property2 = 17;
            Assert.NotEqual(vm.Property2.ToString(), view.SomeTextBox.Text);

            var disp = fixture.Bind(vm, view, x => x.Property2, x => x.SomeTextBox.Text, (IObservable<Unit>)null, null);

            Assert.Equal(vm.Property2.ToString(), view.SomeTextBox.Text);
            Assert.Equal(17, vm.Property2);

            view.SomeTextBox.Text = "42";
            Assert.Equal(42, vm.Property2);

            // Bad formatting error
            view.SomeTextBox.Text = "--";
            Assert.Equal(42, vm.Property2);

            disp.Dispose();
            vm.Property2 = 0;

            Assert.Equal(0, vm.Property2);
            Assert.NotEqual("0", view.SomeTextBox.Text);

            vm.JustADecimal = 17.2m;
            var disp1 = fixture.Bind(vm, view, x => x.JustADecimal, x => x.SomeTextBox.Text, (IObservable<Unit>)null, null);

            Assert.Equal(vm.JustADecimal.ToString(), view.SomeTextBox.Text);
            Assert.Equal(17.2m, vm.JustADecimal);

            view.SomeTextBox.Text = 42.3m.ToString();
            Assert.Equal(42.3m, vm.JustADecimal);

            // Bad formatting.
            view.SomeTextBox.Text = "--";
            Assert.Equal(42.3m, vm.JustADecimal);

            disp1.Dispose();

            vm.JustADecimal = 0;

            Assert.Equal(0, vm.JustADecimal);
            Assert.NotEqual("0", view.SomeTextBox.Text);

            // Empty test
            vm.JustAInt32 = 12;
            var disp2 = fixture.Bind(vm, view, x => x.JustAInt32, x => x.SomeTextBox.Text, (IObservable<Unit>)null, null);

            view.SomeTextBox.Text = string.Empty;
            Assert.Equal(12, vm.JustAInt32);

            view.SomeTextBox.Text = "1.2";

            Assert.Equal(12, vm.JustAInt32);

            view.SomeTextBox.Text = "13";
            Assert.Equal(13, vm.JustAInt32);
        }

        [WpfFact]
        public void BindingIntoModelObjects()
        {
            var vm = new PropertyBindViewModel();
            var view = new PropertyBindView { ViewModel = vm };

            view.OneWayBind(view.ViewModel, x => x.Model.AnotherThing, x => x.SomeTextBox.Text);
            Assert.Equal("Baz", view.SomeTextBox.Text);
        }

        [WpfFact]
        public void ViewModelNullableToViewNonNullable()
        {
            var vm = new PropertyBindViewModel();
            var view = new PropertyBindView { ViewModel = vm };

            view.Bind(view.ViewModel, x => x.NullableDouble, x => x.FakeControl.JustADouble);
            Assert.Equal(0.0, view.FakeControl.JustADouble);

            vm.NullableDouble = 4.0;
            Assert.Equal(4.0, view.FakeControl.JustADouble);

            vm.NullableDouble = null;
            Assert.Equal(4.0, view.FakeControl.JustADouble);

            vm.NullableDouble = 0.0;
            Assert.Equal(0.0, view.FakeControl.JustADouble);
        }

        [WpfFact]
        public void ViewModelNonNullableToViewNullable()
        {
            var vm = new PropertyBindViewModel();
            var view = new PropertyBindView { ViewModel = vm };

            view.Bind(view.ViewModel, x => x.JustADouble, x => x.FakeControl.NullableDouble);
            Assert.Equal(0.0, vm.JustADouble);

            view.FakeControl.NullableDouble = 4.0;
            Assert.Equal(4.0, vm.JustADouble);

            view.FakeControl.NullableDouble = null;
            Assert.Equal(4.0, vm.JustADouble);

            view.FakeControl.NullableDouble = 0.0;
            Assert.Equal(0.0, vm.JustADouble);
        }

        [WpfFact]
        public void ViewModelNullableToViewNullable()
        {
            var vm = new PropertyBindViewModel();
            var view = new PropertyBindView { ViewModel = vm };

            view.Bind(view.ViewModel, x => x.NullableDouble, x => x.FakeControl.NullableDouble);
            Assert.Equal(null, vm.NullableDouble);

            view.FakeControl.NullableDouble = 4.0;
            Assert.Equal(4.0, vm.NullableDouble);

            view.FakeControl.NullableDouble = null;
            Assert.Equal(null, vm.NullableDouble);

            view.FakeControl.NullableDouble = 0.0;
            Assert.Equal(0.0, vm.NullableDouble);
        }

        [WpfFact]
        public void ViewModelIndexerToView()
        {
            var vm = new PropertyBindViewModel();
            var view = new PropertyBindView { ViewModel = vm };

            view.OneWayBind(view.ViewModel, x => x.SomeCollectionOfStrings[0], x => x.SomeTextBox.Text);
            Assert.Equal("Foo", view.SomeTextBox.Text);
        }

        [WpfFact]
        public void ViewModelIndexerToViewChanges()
        {
            var vm = new PropertyBindViewModel();
            var view = new PropertyBindView { ViewModel = vm };

            view.OneWayBind(view.ViewModel, x => x.SomeCollectionOfStrings[0], x => x.SomeTextBox.Text);
            Assert.Equal("Foo", view.SomeTextBox.Text);

            vm.SomeCollectionOfStrings[0] = "Bar";

            Assert.Equal("Bar", view.SomeTextBox.Text);
        }

        [WpfFact]
        public void ViewModelIndexerPropertyToView()
        {
            var vm = new PropertyBindViewModel();
            var view = new PropertyBindView { ViewModel = vm };

            view.OneWayBind(view.ViewModel, x => x.SomeCollectionOfStrings[0].Length, x => x.SomeTextBox.Text);
            Assert.Equal("3", view.SomeTextBox.Text);
        }

        [WpfFact]
        public void BindToShouldntInitiallySetToNull()
        {
            var vm = new PropertyBindViewModel();
            var view = new PropertyBindView { ViewModel = null };

            view.OneWayBind(vm, x => x.Model.AnotherThing, x => x.FakeControl.NullHatingString);
            Assert.Equal(string.Empty, view.FakeControl.NullHatingString);

            view.ViewModel = vm;
            Assert.Equal(vm.Model.AnotherThing, view.FakeControl.NullHatingString);
        }

        [WpfFact]
        public void BindToTypeConversionSmokeTest()
        {
            var vm = new PropertyBindViewModel();
            var view = new PropertyBindView { ViewModel = null };

            view.WhenAnyValue(x => x.ViewModel.JustADouble)
                .BindTo(view, x => x.FakeControl.NullHatingString);

            Assert.Equal(string.Empty, view.FakeControl.NullHatingString);

            view.ViewModel = vm;
            Assert.Equal(vm.JustADouble.ToString(), view.FakeControl.NullHatingString);
        }

        [WpfFact]
        public void BindToNullShouldThrowHelpfulError()
        {
            var view = new PropertyBindView { ViewModel = null };

            Assert.Throws<ArgumentNullException>(() =>
                 view.WhenAnyValue(x => x.FakeControl.NullHatingString)
                     .BindTo(view.ViewModel, x => x.Property1));
        }

#if !MONO
        [WpfFact]
        public void TwoWayBindToSelectedItemOfItemsControl()
        {
            var vm = new PropertyBindViewModel();
            var view = new PropertyBindView { ViewModel = vm };
            view.FakeItemsControl.ItemsSource = new ObservableCollectionExtended<string>(new[] { "aaa", "bbb", "ccc" });

            view.Bind(view.ViewModel, x => x.Property1, x => x.FakeItemsControl.SelectedItem);

            Assert.Null(view.FakeItemsControl.SelectedItem);
            Assert.Null(vm.Property1);

            view.FakeItemsControl.SelectedItem = "aaa";
            Assert.Equal("aaa", vm.Property1); // fail

            vm.Property1 = "bbb";
            Assert.Equal("bbb", view.FakeItemsControl.SelectedItem);
        }

        [WpfFact]
        public void ItemsControlShouldGetADataTemplate()
        {
            var vm = new PropertyBindViewModel();
            var view = new PropertyBindView { ViewModel = vm };

            Assert.Null(view.FakeItemsControl.ItemTemplate);
            view.OneWayBind(vm, x => x.SomeCollectionOfStrings, x => x.FakeItemsControl.ItemsSource);

            Assert.NotNull(view.FakeItemsControl.ItemTemplate);
        }

        [WpfFact]
        public void ItemsControlWithDisplayMemberPathSetShouldNotGetADataTemplate()
        {
            var vm = new PropertyBindViewModel();
            var view = new PropertyBindView { ViewModel = vm };
            view.FakeItemsControl.DisplayMemberPath = "Bla";

            Assert.Null(view.FakeItemsControl.ItemTemplate);
            view.OneWayBind(vm, x => x.SomeCollectionOfStrings, x => x.FakeItemsControl.ItemsSource);

            Assert.Null(view.FakeItemsControl.ItemTemplate);
        }

        [WpfFact]
        public void ItemsControlShouldGetADataTemplateInBindTo()
        {
            var vm = new PropertyBindViewModel();
            var view = new PropertyBindView { ViewModel = vm };

            Assert.Null(view.FakeItemsControl.ItemTemplate);
            vm.WhenAnyValue(x => x.SomeCollectionOfStrings)
                .BindTo(view, v => v.FakeItemsControl.ItemsSource);

            Assert.NotNull(view.FakeItemsControl.ItemTemplate);

            view.WhenAnyValue(x => x.FakeItemsControl.SelectedItem)
                .BindTo(vm, x => x.Property1);
        }

        [WpfFact]
        public void BindingToItemsControl()
        {
            var vm = new PropertyBindViewModel();
            var view = new PropertyBindView { ViewModel = vm };

            view.OneWayBind(view.ViewModel, x => x.SomeCollectionOfStrings, x => x.FakeItemsControl.ItemsSource);
            Assert.True(view.FakeItemsControl.ItemsSource.OfType<string>().Count() > 1);
        }
#endif

        [WpfFact]
        public void BindExpectsConverterFuncsToNotBeNull()
        {
            var vm = new PropertyBindViewModel();
            var view = new PropertyBindView { ViewModel = vm };
            var fixture = new PropertyBinderImplementation();

            Func<string, string> nullFunc = null;

            Assert.Throws<ArgumentNullException>(() => fixture.Bind(vm, view, x => x.Property1, x => x.SomeTextBox.Text, (IObservable<Unit>)null, nullFunc, s => s));
            Assert.Throws<ArgumentNullException>(() => fixture.Bind(vm, view, x => x.Property1, x => x.SomeTextBox.Text, (IObservable<Unit>)null, s => s, nullFunc));
        }

        [WpfFact]
        public void BindWithFuncShouldWorkAsExtensionMethodSmokeTest()
        {
            var vm = new PropertyBindViewModel();
            var view = new PropertyBindView { ViewModel = vm };

            view.Bind(vm, x => x.JustADecimal, x => x.SomeTextBox.Text, d => d.ToString(), decimal.Parse);
        }
    }
}
