using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace VendTech.BLL.Models
{
    public class PlatformModel
    {
        public int PlatformId { get; set; }
        public string ShortName { get; set; }
        public string Title { get; set; }
        public bool Enabled { get; set; }
        public string Logo { get; set; }
        public decimal MinimumAmount { get; set; }
        public bool DisablePlatform { get; set; }
        public string DiabledPlaformMessage { get; set; }
        public int PlatformType { get; set; }
        public int? PlatformApiConnId { get; set; }
        public string PlatformApiConnName { get; set; }

        private string _typeName;
        public string PlatformTypeName
        {
            get
            {
                if (string.IsNullOrEmpty(_typeName))
                {
                    _typeName = GetPlatformTypeName(PlatformType);
                }

                return _typeName;
            }
            set { _typeName = value; }
        }

        public static string GetPlatformTypeName(int PlatformType)
        {
            try
            {
                PlatformTypeEnum pe = (PlatformTypeEnum)PlatformType;
                return pe.ToString();
            }
            catch (Exception ex)
            {
                return "Invalid Platform Type";
            }
        }

        private static NameValueModel[] PLATFORM_TYPES = null;
        private static object lockObj = new object();

        public static NameValueModel[] GetPlatformTypes()
        {
            if (PLATFORM_TYPES == null)
            {
                lock (lockObj)
                {
                    if (PLATFORM_TYPES == null)
                    {
                        var list = new List<NameValueModel>();
                        foreach (PlatformTypeEnum val in EnumUtils.GetEnumValues<PlatformTypeEnum>())
                        {
                            list.Add(new NameValueModel
                            {
                                Name = val.ToString(),
                                Value = ((int)val).ToString()
                            });
                        }
                        PLATFORM_TYPES = list.ToArray();
                    }

                }
            }

            return PLATFORM_TYPES;
        }

        public static bool IsElectricity(int pPlaformType)
        {
            return (pPlaformType == (int)PlatformTypeEnum.ELECTRICITY);
        }

        public bool IsElectricity()
        {
            return (PlatformType == (int) PlatformTypeEnum.ELECTRICITY);
        }

        public bool IsAirtime()
        {
            return (PlatformType == (int)PlatformTypeEnum.AIRTIME);
        }
        public static bool IsAirtime(int pPlatformType)
        {
            return (pPlatformType == (int) PlatformTypeEnum.AIRTIME);
        }

        public static bool IsCableTv(int pPlatformType)
        { 
            return (pPlatformType == (int)PlatformTypeEnum.CABLE_TV);
        }

        public bool IsCableTv()
        {
            return (PlatformType == (int)PlatformTypeEnum.CABLE_TV);
        }
       
        public static List<SelectListItem> ConvertToSelectListItems(ICollection<PlatformModel> list)
        {
            if (list == null) return new List<SelectListItem>(0);

            List<SelectListItem> resp = new List<SelectListItem>(list.Count);
            foreach (PlatformModel p in list)
            {
                resp.Add(new SelectListItem { Text = p.Title, Value = p.PlatformId.ToString() });
            }

            return resp;
        }
    }

    public class SavePlatformModel
    {
        public int? Id { get; set; }
        public string ShortName { get; set; }
        public string Title { get; set; }
        public HttpPostedFile Image { get; set; }
        public HttpPostedFileBase ImagefromWeb { get; set; }
        public string Logo { get; internal set; }
        public decimal MinimumAmount { get; set; }
        public bool DisablePlatform { get; set; } = false;
        public string DiabledPlaformMessage { get; set; }
        public int PlatformType { get; set; }
        public int? PlatformApiConnId { get; set; }
        
    }
}
