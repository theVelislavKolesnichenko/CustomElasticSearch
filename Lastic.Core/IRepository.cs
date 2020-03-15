using Lastic.Core.Models;
using Nest;
using System.Collections.Generic;

namespace Lastic.Core
{
    public interface IRepository<T> where T : BaseItem
    {
        /// <summary>
        ///   Get the total items count.
        /// </summary>
        long CountItems { get; }

        /// <summary>
        ///   Gets all items from repo index
        /// </summary>
        IEnumerable<T> All();

        T GetById(int id);

        /// <summary>
        /// Add a new item to repo index.
        /// </summary>
        /// <param name="item">Specified a new item to create. </param>
        /// /// <param name="immediately">By default, an index shard uses a refresh interval of one second, 
        ///     i.e., new documents become available for search after one second.
        /// </param>
        /// <returns>true if succesfully added.</returns>
        IIndexResponse Add(T item, bool immediately = false);

        /// <summary>
        /// Add new items to repo index.
        /// </summary>
        /// <param name="items">Specified new items to create.</param>
        /// <param name="immediately">By default, an index shard uses a refresh interval of one second, 
        ///     i.e., new documents become available for search after one second.
        /// </param>
        /// <returns>true if succesfully added.</returns>
        bool Add(IEnumerable<T> items, bool immediately = false);

        /// <summary>
        /// Deletes the item by primary key
        /// </summary>
        /// <param name="id"> </param>
        bool Delete(int id);

        /// <summary>
        /// Deletes the item by primary key
        /// </summary>
        /// <param name="id"> </param>
        bool Delete(IEnumerable<int> id);

        /// <summary>
        /// Delete the item from repo index.
        /// </summary>
        /// <param name="item">Specified a existing item to delete. </param>
        void Delete(T item);

        /// <summary>
        /// Update item changes and save to repo index.
        /// </summary>
        /// <param name="item">Specified the item to save. </param>
        void Update(T item);
    }
}
