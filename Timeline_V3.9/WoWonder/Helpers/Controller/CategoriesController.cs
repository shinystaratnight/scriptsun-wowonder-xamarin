using System;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;

namespace WoWonder.Helpers.Controller
{
    public class CategoriesController
    { 
        public static ObservableCollection<Classes.Categories> ListCategoriesPage = new ObservableCollection<Classes.Categories>();
        public static ObservableCollection<Classes.Categories> ListCategoriesGroup = new ObservableCollection<Classes.Categories>();
        public static ObservableCollection<Classes.Categories> ListCategoriesBlog = new ObservableCollection<Classes.Categories>();
        public static ObservableCollection<Classes.Categories> ListCategoriesProducts = new ObservableCollection<Classes.Categories>();
        public static ObservableCollection<Classes.Categories> ListCategoriesJob = new ObservableCollection<Classes.Categories>();
        public static ObservableCollection<Classes.Categories> ListCategoriesMovies = new ObservableCollection<Classes.Categories>();

        public string Get_Translate_Categories_Communities(string idCategory, string textCategory , string type)
        {
            try
            {
                string categoryName = textCategory;

                switch (type)
                {
                    case "Page":
                    {
                        if (ListCategoriesPage?.Count > 0)
                            categoryName = ListCategoriesPage.FirstOrDefault(a => a.CategoriesId == idCategory)?.CategoriesName ?? textCategory;
                        break;
                    }
                    case "Group":
                    {
                        if (ListCategoriesGroup?.Count > 0)
                            categoryName = ListCategoriesGroup.FirstOrDefault(a => a.CategoriesId == idCategory)?.CategoriesName ?? textCategory;
                        break;
                    }
                    case "Blog":
                    {
                        if (ListCategoriesBlog?.Count > 0)
                            categoryName = ListCategoriesBlog.FirstOrDefault(a => a.CategoriesId == idCategory)?.CategoriesName ?? textCategory;
                        break;
                    }
                    case "Products":
                    {
                        if (ListCategoriesProducts?.Count > 0)
                            categoryName = ListCategoriesProducts.FirstOrDefault(a => a.CategoriesId == idCategory)?.CategoriesName ?? textCategory;
                        break;
                    }
                    case "Job":
                    {
                        if (ListCategoriesJob?.Count > 0)
                            categoryName = ListCategoriesJob.FirstOrDefault(a => a.CategoriesId == idCategory)?.CategoriesName ?? textCategory;
                        break;
                    }
                    case "Movies":
                    {
                        if (ListCategoriesMovies?.Count > 0)
                            categoryName = ListCategoriesMovies.FirstOrDefault(a => a.CategoriesId == idCategory)?.CategoriesName ?? textCategory;
                        break;
                    }
                    default:
                        categoryName = Application.Context.GetText(Resource.String.Lbl_Unknown);
                        break;
                }

                if (string.IsNullOrEmpty(categoryName))
                    return Application.Context.GetText(Resource.String.Lbl_Unknown);
                 
                return categoryName; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);

                if (string.IsNullOrEmpty(textCategory))
                    return Application.Context.GetText(Resource.String.Lbl_Unknown);

                return textCategory;
            }
        } 
    }
}