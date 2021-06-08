﻿using Examine;
using Examine.Lucene.Providers;
using Lucene.Net.Index;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Services;

namespace UmbracoExamine.PDF
{
    /// <summary>
    /// Component to index PDF documents in the media library
    /// </summary>
    public class ExaminePdfComponent : IComponent //TODO: Change to ExamineUserComponent for 8.1.2
    {
        private readonly IExamineManager _examineManager;
        private readonly PdfIndexCreator _pdfIndexCreator;
        private readonly PdfIndexPopulator _pdfIndexPopulator;
        private readonly IMediaService _mediaService;
        private readonly ILogger<ExaminePdfComponent> _logger;

        public ExaminePdfComponent(
            IExamineManager examineManager,
            PdfIndexCreator pdfIndexCreator,
            PdfIndexPopulator pdfIndexPopulator,
            IMediaService mediaService,
            ILogger<ExaminePdfComponent> logger)
        {
            _examineManager = examineManager;
            _pdfIndexCreator = pdfIndexCreator;
            _pdfIndexPopulator = pdfIndexPopulator;
            _mediaService = mediaService;
            _logger = logger;
        }

        public void Initialize()
        {
            // TODO (V9) Replace this check if necessary.
            // //TODO: Remove this entire check for 8.1.2
            // var examineEnabled = _mainDom.Register(() => {});
            // if (!examineEnabled) return;

            foreach (var index in _pdfIndexCreator.Create())
            {
                //TODO: Remove this block for 8.1.2 since Umbraco will ensure the locks are removed
                if (index is LuceneIndex luceneIndex)
                {
                    var dir = luceneIndex.GetLuceneDirectory();
                    if (IndexWriter.IsLocked(dir))
                    {
                        _logger.LogInformation(
                            "Forcing index {IndexerName} to be unlocked since it was left in a locked state",
                            luceneIndex.Name);
                        IndexWriter.Unlock(dir);
                    }
                }

                // TODO (V9): Figure out the alternative to this
                // _examineManager.AddIndex(index);
            }

            // TODO(V9) replace with notification handler
            //MediaCacheRefresher.CacheUpdated += MediaCacheRefresherUpdated;
        }

        public void Terminate()
        {
        }

        // /// <summary>
        // /// Handle the cache refresher event to update the index
        // /// </summary>
        // /// <param name="sender"></param>
        // /// <param name="args"></param>
        // private void MediaCacheRefresherUpdated(MediaCacheRefresher sender, CacheRefresherEventArgs args)
        // {
        //     if (args.MessageType != MessageType.RefreshByPayload)
        //         throw new NotSupportedException();
        //
        //     foreach (var payload in (MediaCacheRefresher.JsonPayload[])args.MessageObject)
        //     {
        //         if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
        //         {
        //             _pdfIndexPopulator.RemoveFromIndex(payload.Id);
        //         }
        //         else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
        //         {
        //             // ExamineEvents does not support RefreshAll
        //             // just ignore that payload
        //             // so what?!
        //         }
        //         else // RefreshNode or RefreshBranch (maybe trashed)
        //         {
        //             var media = _mediaService.GetById(payload.Id);
        //             if (media == null)
        //             {
        //                 // gone fishing, remove entirely
        //                 _pdfIndexPopulator.RemoveFromIndex(payload.Id);
        //                 continue;
        //             }
        //
        //             if (media.Trashed)
        //                 _pdfIndexPopulator.RemoveFromIndex(payload.Id);
        //             else
        //                 _pdfIndexPopulator.AddToIndex(media);
        //
        //             // branch
        //             if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
        //             {
        //                 const int pageSize = 500;
        //                 var page = 0;
        //                 var total = long.MaxValue;
        //                 while (page * pageSize < total)
        //                 {
        //                     var descendants = _mediaService.GetPagedDescendants(media.Id, page++, pageSize, out total);
        //                     foreach (var descendant in descendants)
        //                     {
        //                         if (descendant.Trashed)
        //                             _pdfIndexPopulator.RemoveFromIndex(descendant);
        //                         else
        //                             _pdfIndexPopulator.AddToIndex(descendant);
        //                     }
        //                 }
        //             }
        //         }
        //     }
        // }


    }
}
