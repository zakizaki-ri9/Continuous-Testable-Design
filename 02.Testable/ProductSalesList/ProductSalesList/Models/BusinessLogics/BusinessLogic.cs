﻿using System.Collections.Generic;
using System.Linq;
using ProductSalesList.Models.Repositories;

namespace ProductSalesList.Models.BusinessLogics
{
    public class BusinessLogic : IBusinessLogic
    {
        private readonly IRepository _repository;

        public BusinessLogic(Repository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// プロダクト別総売上額のリストを売上額の多い順に取得する
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ProductSales> GetProductSalesList()
        {
            // 注文明細を全て取得し、プロダクトIDでグルーピングし、プロダクトID別の総売上を集計する
            var salesByProductId =
                _repository.GetSalesOrderDetail()
                    .GroupBy(salesOrderDetail => salesOrderDetail.ProductID)
                    .Select(x =>
                        new
                        {
                            ProductID = x.Key,
                            TotalAmount = x.Sum(salesOrderDetail => salesOrderDetail.LineTotal)
                        });
            // 全てのプロダクトを取得し、先に集計したプロダクトID別の総売上とJoinする。
            // その上で、プロダクト名別 総売上リストを作成し、売上額の多い順にソートする。
            return
                _repository.GetProducts()
                    .Join(
                        salesByProductId,
                        p => p.ProductID,
                        s => s.ProductID,
                        (p, s) => 
                            new ProductSales
                            {
                                Name = p.Name,
                                Sales = s.TotalAmount
                            })
                    .OrderByDescending(productSales => productSales.Sales);
        }
    }
}
