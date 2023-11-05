using ProximitySlides.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProximitySlides.App.Pages;

public partial class TestPage : ContentPage
{
    public TestPage(TestViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}