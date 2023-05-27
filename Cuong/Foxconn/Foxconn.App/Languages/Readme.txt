1. Create a file for language 
1.1 English.xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:sys="clr-namespace:System;assembly=mscorlib">
	<sys:String x:Key="Languages">Language</sys:String>
	<sys:String x:Key="English">English</sys:String>
    <sys:String x:Key="Vietnamese">Vietnamese</sys:String>
    <sys:String x:Key="Hello">Hello</sys:String>
</ResourceDictionary>

1.2 Vietnamese.xaml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:sys="clr-namespace:System;assembly=mscorlib">
	<sys:String x:Key="Languages">Ngôn ngữ</sys:String>
    <sys:String x:Key="English">Tiếng Anh</sys:String>
    <sys:String x:Key="Vietnamese">Tiếng Việt</sys:String>
    <sys:String x:Key="Hello">Xin chào</sys:String>
</ResourceDictionary>

2. Add code below to App.xaml
<ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
        <!--<ResourceDictionary Source="/Languages/Vietnamese.xaml"/>-->
        <ResourceDictionary Source="/Languages/English.xaml"/>
    </ResourceDictionary.MergedDictionaries>
</ResourceDictionary>

3. MainWindow.xaml
<MenuItem x:Name="mnuiLanguages" Header="{DynamicResource ResourceKey=Languages}">
    <MenuItem x:Name="mnuiEnglish" Click="mnuiEnglish_Click" Header="{DynamicResource ResourceKey=English}"/>
    <MenuItem x:Name="mnuiVietnamese" Click="mnuiVietnamese_Click" Header="{DynamicResource ResourceKey=Vietnamese}"/>
</MenuItem>

4. MainWindow.cs
private void mnuiEnglish_Click(object sender, RoutedEventArgs e)
{
    MyLanguage.Apply(this, "en-US");
    MessageBox.Show(MyResources.GetString("en-US", "Hello"));
}

private void mnuiVietnamese_Click(object sender, RoutedEventArgs e)
{
    MyLanguage.Apply(this, "vi-VN");
    MessageBox.Show(MyResources.GetString("vi-VN", "Hello"));
}