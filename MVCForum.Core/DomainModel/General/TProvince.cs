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
            ProvinceList.Add(new TProvince() { ProvinceId = 110000, ProvinceName = "������" });
            ProvinceList.Add(new TProvince() { ProvinceId = 120000, ProvinceName = "�����" });
            ProvinceList.Add(new TProvince() { ProvinceId = 130000, ProvinceName = "�ӱ�ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 140000, ProvinceName = "ɽ��ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 150000, ProvinceName = "���ɹ�" });
            ProvinceList.Add(new TProvince() { ProvinceId = 210000, ProvinceName = "����ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 220000, ProvinceName = "����ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 230000, ProvinceName = "������" });
            ProvinceList.Add(new TProvince() { ProvinceId = 310000, ProvinceName = "�Ϻ���" });
            ProvinceList.Add(new TProvince() { ProvinceId = 320000, ProvinceName = "����ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 330000, ProvinceName = "�㽭ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 340000, ProvinceName = "����ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 350000, ProvinceName = "����ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 360000, ProvinceName = "����ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 370000, ProvinceName = "ɽ��ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 410000, ProvinceName = "����ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 420000, ProvinceName = "����ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 430000, ProvinceName = "����ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 440000, ProvinceName = "�㶫ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 450000, ProvinceName = "����" });
            ProvinceList.Add(new TProvince() { ProvinceId = 460000, ProvinceName = "����ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 500000, ProvinceName = "������" });
            ProvinceList.Add(new TProvince() { ProvinceId = 510000, ProvinceName = "�Ĵ�ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 520000, ProvinceName = "����ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 530000, ProvinceName = "����ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 540000, ProvinceName = "����" });
            ProvinceList.Add(new TProvince() { ProvinceId = 610000, ProvinceName = "����ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 620000, ProvinceName = "����ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 630000, ProvinceName = "�ຣʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 640000, ProvinceName = "����" });
            ProvinceList.Add(new TProvince() { ProvinceId = 650000, ProvinceName = "�½�" });
            ProvinceList.Add(new TProvince() { ProvinceId = 710000, ProvinceName = "̨��ʡ" });
            ProvinceList.Add(new TProvince() { ProvinceId = 810000, ProvinceName = "����ر�������" });
            ProvinceList.Add(new TProvince() { ProvinceId = 820000, ProvinceName = "�����ر�������" });
            return ProvinceList;
        }
    }

}