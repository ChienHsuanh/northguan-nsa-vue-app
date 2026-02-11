using Microsoft.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Data;
using northguan_nsa_vue_app.Server.DTOs;

namespace northguan_nsa_vue_app.Server.Services.Base
{
    /// <summary>
    /// 記錄服務基類，提供通用的分頁和查詢功能
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <typeparam name="TResponse">響應DTO類型</typeparam>
    /// <typeparam name="TQueryParams">查詢參數類型</typeparam>
    public abstract class BaseRecordService<TEntity, TResponse, TQueryParams>
        where TEntity : class
        where TResponse : class
        where TQueryParams : RecordQueryParameters
    {
        protected readonly ApplicationDbContext _context;

        protected BaseRecordService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 獲取記錄列表
        /// </summary>
        public async Task<PagedResponse<TResponse>> GetRecordsListAsync(TQueryParams parameters)
        {
            var deviceSerials = await GetAvailableDeviceSerialsAsync(parameters.StationIds ?? new List<int>(), parameters.Keyword);
            
            var query = BuildBaseQuery(deviceSerials);
            query = ApplyFilters(query, parameters);
            
            var totalCount = await query.CountAsync();
            
            query = ApplySorting(query, parameters);
            query = ApplyPagination(query, parameters);
            
            var entities = await query.ToListAsync();
            var responses = await MapToResponsesAsync(entities);
            
            return CreatePagedResponse(responses, totalCount, parameters);
        }

        /// <summary>
        /// 構建基礎查詢
        /// </summary>
        protected abstract IQueryable<TEntity> BuildBaseQuery(List<string> deviceSerials);

        /// <summary>
        /// 應用過濾條件
        /// </summary>
        protected virtual IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> query, TQueryParams parameters)
        {
            // 應用日期過濾
            query = ApplyDateFilters(query, parameters);
            
            // 應用特定過濾條件
            query = ApplySpecificFilters(query, parameters);
            
            return query;
        }

        /// <summary>
        /// 應用日期過濾
        /// </summary>
        protected abstract IQueryable<TEntity> ApplyDateFilters(IQueryable<TEntity> query, TQueryParams parameters);

        /// <summary>
        /// 應用特定過濾條件
        /// </summary>
        protected abstract IQueryable<TEntity> ApplySpecificFilters(IQueryable<TEntity> query, TQueryParams parameters);

        /// <summary>
        /// 應用排序
        /// </summary>
        protected abstract IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, TQueryParams parameters);

        /// <summary>
        /// 應用分頁
        /// </summary>
        protected virtual IQueryable<TEntity> ApplyPagination(IQueryable<TEntity> query, TQueryParams parameters)
        {
            return query
                .Skip((parameters.Page - 1) * parameters.Size)
                .Take(parameters.Size);
        }

        /// <summary>
        /// 映射到響應DTO
        /// </summary>
        protected abstract Task<List<TResponse>> MapToResponsesAsync(List<TEntity> entities);

        /// <summary>
        /// 獲取可用設備序號
        /// </summary>
        protected abstract Task<List<string>> GetAvailableDeviceSerialsAsync(List<int> availableStationIds, string keyword);

        /// <summary>
        /// 創建分頁響應
        /// </summary>
        protected virtual PagedResponse<TResponse> CreatePagedResponse(List<TResponse> data, int totalCount, TQueryParams parameters)
        {
            var totalPages = (int)Math.Ceiling((double)totalCount / parameters.Size);

            return new PagedResponse<TResponse>
            {
                Data = data,
                TotalCount = totalCount,
                Page = parameters.Page,
                Size = parameters.Size,
                TotalPages = totalPages,
                HasNextPage = parameters.Page < totalPages,
                HasPreviousPage = parameters.Page > 1,
                Success = true
            };
        }

        /// <summary>
        /// 獲取記錄總數
        /// </summary>
        public async Task<int> GetRecordsCountAsync(string keyword, List<int> availableStationIds)
        {
            var deviceSerials = await GetAvailableDeviceSerialsAsync(availableStationIds, keyword);
            var query = BuildBaseQuery(deviceSerials);
            return await query.CountAsync();
        }

        /// <summary>
        /// 導出記錄
        /// </summary>
        public abstract Task<byte[]> ExportRecordsAsync(string keyword, List<int> availableStationIds);
    }
}