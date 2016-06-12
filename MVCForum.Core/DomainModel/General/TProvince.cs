using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MVCForum.Domain.DomainModel.General
{

    public class TProvince
    {
        public int ProvinceId;
        public string ProvinceName;


        public static List<TProvince> LoadAllProvinceList()
        {
            var ProvinceList = new List<TProvince>();
            ProvinceList.Add(new TProvince() { ProvinceId = 110000, ProvinceName = "北京市" });
            ProvinceList.Add(new TProvince() { ProvinceId = 120000, ProvinceName = "天津市" });
            ProvinceList.Add(new TProvince() { ProvinceId = 130000, ProvinceName = "河北省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 140000, ProvinceName = "山西省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 150000, ProvinceName = "内蒙古" });
            ProvinceList.Add(new TProvince() { ProvinceId = 210000, ProvinceName = "辽宁省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 220000, ProvinceName = "吉林省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 230000, ProvinceName = "黑龙江" });
            ProvinceList.Add(new TProvince() { ProvinceId = 310000, ProvinceName = "上海市" });
            ProvinceList.Add(new TProvince() { ProvinceId = 320000, ProvinceName = "江苏省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 330000, ProvinceName = "浙江省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 340000, ProvinceName = "安徽省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 350000, ProvinceName = "福建省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 360000, ProvinceName = "江西省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 370000, ProvinceName = "山东省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 410000, ProvinceName = "河南省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 420000, ProvinceName = "湖北省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 430000, ProvinceName = "湖南省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 440000, ProvinceName = "广东省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 450000, ProvinceName = "广西" });
            ProvinceList.Add(new TProvince() { ProvinceId = 460000, ProvinceName = "海南省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 500000, ProvinceName = "重庆市" });
            ProvinceList.Add(new TProvince() { ProvinceId = 510000, ProvinceName = "四川省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 520000, ProvinceName = "贵州省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 530000, ProvinceName = "云南省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 540000, ProvinceName = "西藏" });
            ProvinceList.Add(new TProvince() { ProvinceId = 610000, ProvinceName = "陕西省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 620000, ProvinceName = "甘肃省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 630000, ProvinceName = "青海省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 640000, ProvinceName = "宁夏" });
            ProvinceList.Add(new TProvince() { ProvinceId = 650000, ProvinceName = "新疆" });
            ProvinceList.Add(new TProvince() { ProvinceId = 710000, ProvinceName = "台湾省" });
            ProvinceList.Add(new TProvince() { ProvinceId = 810000, ProvinceName = "香港特别行政区" });
            ProvinceList.Add(new TProvince() { ProvinceId = 820000, ProvinceName = "澳门特别行政区" });
            return ProvinceList;
        }
    }

}