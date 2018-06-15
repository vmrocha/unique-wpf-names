# Uniquely naming WPF elements

Associating dynamically created UI elements in WPF with unique names.

## Motivation

Here at [Clemex](https://www.clemex.com/), we use an automated testing tool that relies on the [`FrameworkElement.Name`](https://msdn.microsoft.com/en-us/library/system.windows.frameworkelement(v=vs.110).aspx) property to interact with the application. Sometimes we need to create dynamic controls based on collection data, but can end up with the same name for multiple UI elements.

For example, the following code generates a menu based on a collection of panels.

```xml
<ItemsControl ItemsSource="{Binding Path=Panels}">
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Button Name="MenuBtn" Content="{Binding Title}" />
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

Inspecting the element tree we would see that we now have multiple elements with the same name: "MenuBtn".

We want to avoid this situation and have unique names for each button.

To accomplish that, we will describe four different approaches.

* Using the Code-Behid: easiest way, but not elegant;
* Using data binding: natural and elegant, but doesn't work;
* Using attached properties: an elegant working solution;
* Using collection indexes: extending our attached properties solution.

## Using the Code-Behind

Assuming that we have access to some unique ID on the elements data context. The easiest approach is to use the `Loaded` event of `FrameworkElement` to set a unique name using the code-behind model.

```xml
<DataTemplate>
    <Button Content="{Binding Title}"
            Loaded="OnMenuBtnLoaded" />
</DataTemplate>
```

```csharp
private void OnMenuBtnLoaded(object sender, RoutedEventArgs e)
{
    if (sender is FrameworkElement element)
    {
        if (element.DataContext is ViewModel item)
        {
            element.Name = $"MenuBtn{item.ID}";
        }
    }
}
```

Now, when we check the element tree, we will see that we have unique names for each button.

## Using data binding

Using data binding makes our code much cleaner, as well as easier to read and understand. If we try a similar approach using data binding we might end up with source code like the following.

```xml
<DataTemplate>
    <Button Content="{Binding Title}"
            Name="{Binding ID, StringFormat='MenuBtn{0}'}" />
</DataTemplate>
```

Unfortunately, if we try to build this code, we will get a compilation error.

> MarkupExtensions are not allowed for Uid or Name property values, so '{Binding Panel.PanelType, StringFormat='MenuBtn{0}'}' is not valid.

This restriction prevents us from binding directly to the `Name` property.

## Using attached properties

To overcome the limitation of the previous attempt we can define a new property that would set the name for us. To add new properties to existing controls we can use [Attached Properties](https://docs.microsoft.com/en-us/dotnet/framework/wpf/advanced/attached-properties-overview).

```csharp
public class AttachedProperties
{
    public static readonly DependencyProperty NameProperty =
        DependencyProperty.RegisterAttached(
            "Name",
            typeof(string),
            typeof(AttachedProperties),
            new FrameworkPropertyMetadata(default(string), OnValueChanged)
        );

    public static void SetName(FrameworkElement element, string value)
    {
        element.SetValue(NameProperty, value);
    }

    public static string GetName(FrameworkElement element)
    {
        return (string)element.GetValue(NameProperty);
    }

    private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            element.Name = (string)element.GetValue(NameProperty);
        }
    }
}
```

The `OnValueChanged` event triggers every time the value of our property changes. When that happens, we get the new value and set it to be the `FrameworkElement` name. We are giving our attached property the name `Name`, but it could be anything we want like `CustomName` or `TestName`.

To use the new property, we need to add a namespace to the XAML and attach the property to our button.

```xml
<UserControl xmlns:ext="clr-namespace:Clemex.Presentation.Extensions">

<DataTemplate>
    <Button Content="{Binding Title}"
            ext:AttachedProperties.Name="{Binding ID, StringFormat='MenuBtn{0}'}" />
</DataTemplate>
```

Our code will now compile without any problems, and we will have unique names for each element.

## Using collection indexes

In the previous example, we created unique names by appending the property `ID`. There are other scenarios where we don't have an ID on the item to create a unique element name. For that, we can instead use the collection index.

Let's try to bind our button collection to a list of strings.

```csharp
public IReadonlyList<string> Values { get; } = new []
{
    "Item 1",
    "Item 2",
    "Item 3"
};
```

To achieve that, we can use the same `AttachedProperty` with a converter. It will look for the index of the element inside the collection.

```csharp
public class IndexOfConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.length == 2 && values[1] is IEnumerable<object> collection)
        {
            return collection.IndexOf(values[0]);
        }
        
        throw new NotImplementedException();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

In the XAML, we will now use [MultiBinding](https://msdn.microsoft.com/en-us/library/system.windows.data.multibinding(v=vs.110).aspx) because we need both the element and the collection.

```xml
<DataTemplate>
    <Button Content="{Binding}">
        <ext:AttachedProperties.Name>
            <MultiBinding StringFormat="MenuBtn{0:00}" Converter="{StaticResource IndexOfConverter}">
                <Binding />
                <Binding Path="DataContext.Values"
                         RelativeSource="{RelativeSource FindAncestor, AncestorType=ListView}" />
            </MultiBinding>
        </ext:AttachedProperties.Name>
    </Button>
</DataTemplate>
```

Looking the element tree we can see that our buttons are named `MenuBtn00`, `MenuBtn01` and so on.

## Summary

Generating unique names for dynamically created WPF controls can be done in a elegant way by using Attached Properties and using the multi-binding with a custom converter.
