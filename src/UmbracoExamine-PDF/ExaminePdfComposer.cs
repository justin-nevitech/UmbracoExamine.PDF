﻿using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Examine;

namespace UmbracoExamine.PDF
{
    /// <summary>
    ///     Registers the ExaminePDFComponent and all of it's injected dependencies
    /// </summary>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class ExaminePdfComposer : ComponentComposer<ExaminePdfComponent>, IUserComposer
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            //Register the services used to make this all work
            composition.RegisterUnique<IPdfTextExtractor, PdfPigTextExtractor>();
            composition.Register<PdfTextService>(Lifetime.Singleton);
            composition.RegisterUnique<IPdfIndexValueSetBuilder, PdfIndexValueSetBuilder>();
            composition.Register<IIndexPopulator, PdfIndexPopulator>(Lifetime.Singleton);
            composition.Register<PdfIndexPopulator>(Lifetime.Singleton);
            composition.RegisterUnique<PdfIndexCreator>();
        }
    }
}
