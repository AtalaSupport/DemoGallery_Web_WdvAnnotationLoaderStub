using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;

using Atalasoft.Imaging.Codec;
using Atalasoft.Annotate;
using Atalasoft.Annotate.UI;
using Atalasoft.Annotate.Formatters;

namespace Atalasoft.Demos
{
    #region Annotation Loader

    /// <summary>
    /// The AnnotationLoader is a fork of our AnnotationConverter class
    /// 
    /// Its purpose is to allow for easy loading WebDocumentViewer annotations into controls 
    /// that take AnnoationUI (WebAnnotationViewer, AnnotateViewer, DocumentAnnotationViewer, 
    /// etc)
    /// 
    /// This is only valid for WebDocumentViewer in 10.6 and newer - for older WDV, you would 
    /// need our "AnnotationConverter" class instead as annotaions need to be scaled to/from 
    /// WDV in 10.5 and older.
    ///
    /// HOW TO USE:
    /// Create a new AnnoationLoader
    /// 
    /// AnnotationLoader loader = new AnnoationLoader();
    /// 
    /// to get a LayerData[] from serialized Desktop or WebAnnotationViewer annotations:
    /// 
    /// LayerData[] ldArray = loader.ToWebDocumentViewer(LayerCollection, "docpath");
    /// or
    /// Size[] pageSizes = ... an array of Size objects representing the physical size in pixels 
    ///                    ...  of each page in the document that LayerCollection represents
    /// LayerData[] ldArray = loader.ToWebDocumentViewer(LayerCollection, pageSizes);
    /// 
    /// 
    /// To get a LayerCollection from WDV annotations 
    /// 
    /// LayerCollection lc = loader.ToAnnotationUI("xmpPath", "documentPath");
    /// or
    /// LayerdData[] ldArray = ... your LayerData[] from WDV annotaitons ...
    /// LayerCollection lc = loader.ToAnnotationUi(ldArray, "documentPath");
    /// 
    /// </summary>
    public class AnnotationLoader
    {
        private AnnotationUIFactoryCollection _factories;

        public AnnotationLoader()
        {
            _factories = new AnnotationUIFactoryCollection();
        }

        public AnnotationLoader(AnnotationUIFactoryCollection factories)
        {
            _factories = factories;
        }

        public AnnotationUIFactoryCollection Factories
        {
            get { return _factories; }
            set { _factories = value; }
        }

        public LayerData[] ToWebDocumentViewer(LayerCollection layerCollection, string docPath)
        {
            Size[] pageSizes = new Size[0];

            if (File.Exists(docPath))
            {
                using (FileStream fs = new FileStream(docPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    pageSizes = new PageSizeAggregate(new DocumentPageSizes(RegisteredDecoders.GetDecoder(fs), fs, false)).ToArray();
                }
            }

            return ToWebDocumentViewer(layerCollection, pageSizes);
        }

        public LayerData[] ToWebDocumentViewer(LayerCollection layerCollection, Size[] pageSizes)
        {
            LayerData[] layers = new LayerData[0];
            if (layerCollection != null)
            {
                layers = LoadAnnotationData(layerCollection);
            }

            // this is where the conversion/scaling into WDV-compatible annoations
            // used to happen. However, we do not need this scaling in 10.6 and newer
            // REMOVED:   ConvertToWDV(layers, pageSizes);

            return layers;
        }

        public LayerCollection ToAnnotationUI(string xmpPath, string docPath)
        {
            LayerData[] layers = new LayerData[0];

            if (File.Exists(xmpPath))
            {
                using (FileStream fs = new FileStream(xmpPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    layers = LoadAnnotationData(fs);
                }
            }

            return ToAnnotationUI(layers, docPath);
        }

        public LayerCollection ToAnnotationUI(LayerData[] layers, string docPath)
        {
            Size[] pageSizes = new Size[0];

            if (File.Exists(docPath))
            {
                using (FileStream fs = new FileStream(docPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    pageSizes = new PageSizeAggregate(new DocumentPageSizes(RegisteredDecoders.GetDecoder(fs), fs, false)).ToArray();
                }
            }

            return ToAnnotationUI(layers, pageSizes);
        }

        public LayerCollection ToAnnotationUI(LayerData[] layers, Size[] pageSizes)
        {
            LayerCollection retLayers = new LayerCollection();

            if (layers != null)
            {
                // we still need a modified Convert just to handle referenced image Annotations
                ConvertFromWDV(layers, pageSizes);

                foreach (LayerData layer in layers)
                {
                    AnnotationUI outAnnot = _factories.GetAnnotationFromData(layer);
                    LayerAnnotation layerAnnot = outAnnot as LayerAnnotation;

                    retLayers.Add(layerAnnot);
                }
            }

            return retLayers;
        }


        #region Annotation Loading

        private LayerData[] LoadAnnotationData(object data)
        {
            LayerData[] layers = null;

            if (data.GetType() == typeof(LayerCollection))
            {
                LayerCollection lc = data as LayerCollection;
                if (lc != null && lc.Count != 0)
                {
                    layers = new LayerData[lc.Count];

                    for (int i = 0; i < layers.Length; i++)
                    {
                        layers[i] = (LayerData)lc[i].Data;
                    }
                }
            }
            else if (data.GetType() == typeof(LayerData[]))
            {
                LayerData[] lda = data as LayerData[];
                if (lda != null)
                {
                    layers = lda;
                }
            }
            else if (data.GetType() == typeof(LayerAnnotation))
            {

                LayerAnnotation la = data as LayerAnnotation;
                if (la != null)
                {
                    layers = new LayerData[1];
                    layers[0] = (LayerData)la.Data;
                }
            } else if (data.GetType() == typeof(LayerData))
            {
                LayerData ld = data as LayerData;
                if (ld != null)
                {
                    layers = new LayerData[1];
                    layers[0] = ld;
                }
            }
            else if (data.GetType() == typeof(AnnotationUI))
            {
                AnnotationUI ann = data as AnnotationUI;
                if (ann != null)
                {
                    layers = new LayerData[1];
                    layers[0].Items.Add(ann.Data);
                }
            } else if (data.GetType() == typeof(AnnotationData))
            {
                AnnotationData ad = data as AnnotationData;
                if (ad != null)
                {
                    layers = new LayerData[1];
                    layers[0].Items.Add(ad);
                }
            }
            return layers;
        }

        private LayerData[] LoadAnnotationData(System.IO.Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            XmpFormatter xmp = new XmpFormatter();
            object data = xmp.Deserialize(stream);

            return LoadAnnotationData(data);
        }


        /// <summary>
        /// In 10.6 and newer, we no longer need to scale annotations from WDV to AnnotationUI
        /// However, we do have to resolve an issue with the ReferecedImageAnnotation objects
        /// used in WDV.. in order to safely convert them to annotaitons compatible with AnnotationUI
        /// we need to convert ReferencedImageAnnoation to EmbeddedImageAnnotation (and that means fetching the annotations)
        /// </summary>
        /// <param name="layers"></param>
        /// <param name="pageSizes"></param>
        private void ConvertFromWDV(LayerData[] layers, Size[] pageSizes)
        {

            for (int i = 0; i < layers.Length; i++)
            {
                for (int j = 0; j < layers[i].Items.Count; j++)
                {
                    AnnotationData data = layers[i].Items[j];

                    if (data is ReferencedImageData)
                    {
                        EmbeddedImageData embeddedImage;

                        embeddedImage = new EmbeddedImageData();
                        embeddedImage.Location = data.Location;
                        embeddedImage.Size = data.Size;
                        try
                        {
                            string refImgPath = ((ReferencedImageData)data).FileName;
                            string fullImgPth = System.Web.HttpContext.Current.Request.MapPath(refImgPath);
                            embeddedImage.Image = new AnnotationImage(new Atalasoft.Imaging.AtalaImage(fullImgPth));

                            layers[i].Items.RemoveAt(j);
                            layers[i].Items.Insert(j, embeddedImage);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.ToString());
                        }
                    }
                    else if (data is TextData)
                    {
                        // we need to scale font size
                        TextData td = data as TextData;
                        td.Font.Size = PixelToPoint((int)td.Font.Size);
                        // NOTE since we're not replacing anything we just need to modify this object and let it be.
                    }
                    //// This used to be needed previous to 10.7.0.11 when RubberStampData was not displaying correctly in WDV
                    //else if (data is RubberStampData)
                    //{
                    //    RubberStampData oldStamp = data as RubberStampData;
                    //    TextData newStamp = new TextData(new RectangleF(oldStamp.Location, oldStamp.Size), oldStamp.Text);
                    //    layers[i].Items.RemoveAt(j);
                    //    layers[i].Items.Insert(j, newStamp);
                    //}
                }
            }
        }

        private float PixelToPoint(int pixel)
        {
            float point = pixel;

            switch (pixel)
            {
                case 8:
                    point = 6.0f;
                    break;
                case 9:
                    point = 7.0f;
                    break;
                case 10:
                    point = 7.5f;
                    break;
                case 11:
                    point = 8.0f;
                    break;
                case 12:
                    point = 9.0f;
                    break;
                case 13:
                    point = 10.0f;
                    break;
                case 14:
                    point = 10.5f;
                    break;
                case 15:
                    point = 11.0f;
                    break;
                case 16:
                    point = 12.0f;
                    break;
                case 17:
                    point = 13.0f;
                    break;
                case 18:
                    point = 13.5f;
                    break;
                case 19:
                    point = 14.0f;
                    break;
                case 20:
                    point = 14.5f;
                    break;
                case 21:
                    point = 15.0f;
                    break;
                case 22:
                    point = 16.0f;
                    break;
                case 23:
                    point = 17.0f;
                    break;
                case 24:
                    point = 18.0f;
                    break;
                case 26:
                    point = 20.0f;
                    break;
                case 29:
                    point = 22.0f;
                    break;
                case 32:
                    point = 24.0f;
                    break;
                case 35:
                    point = 26.0f;
                    break;
                case 36:
                    point = 27.0f;
                    break;
                case 37:
                    point = 28.0f;
                    break;
                case 38:
                    point = 29.0f;
                    break;
                case 40:
                    point = 30.0f;
                    break;
                case 42:
                    point = 32.0f;
                    break;
                case 45:
                    point = 34.0f;
                    break;
                case 48:
                    point = 36.0f;
                    break;
                default:
                    point = (float)0.70 * pixel;
                    break;
            }

            return point;
        }

        #endregion

        #region Page Size Helpers

        public class SingleMultiple<T>
        {
            private List<T> _coll = new List<T>();
            protected T Single { get { return _coll[0]; } set { _coll.Clear(); _coll.Add(value); } }
            protected IList<T> Multiple { get { return _coll; } }
        }

        public class PageSizeAggregate : SingleMultiple<Size>
        {
            private int _count;

            public PageSizeAggregate(Size size)
                : base()
            {
                this.Single = size;
                this._count = 1;
            }

            public PageSizeAggregate(IEnumerable<Size> sizes)
                : base()
            {
                this._count = 0;

                foreach (Size s in sizes)
                {
                    Multiple.Add(s);
                    this._count++;
                }

                if (this._count <= 0)
                    throw new ArgumentOutOfRangeException("sizes", "sizes must have at least one entry");
            }

            public IEnumerable<Size> GetEnumerator()
            {
                foreach (Size s in Multiple)
                {
                    yield return s;
                }
            }

            public int Count
            {
                get { return this._count; }
            }

            public Size[] ToArray()
            {
                Size[] sizes = new Size[this._count];
                Multiple.CopyTo(sizes, 0);
                return sizes;
            }
        }

        public class DocumentPageSizes : IEnumerable<Size>
        {
            #region IEnumerable<Size> Members

            Stream _stm;
            ImageDecoder _dec;
            int _count;
            bool _timeoutPages = true;

            public DocumentPageSizes(ImageDecoder dec, Stream stm, bool timeoutPages)
            {
                _stm = stm;
                _dec = dec;
                _count = -1;
                _timeoutPages = timeoutPages;
            }

            public int Count
            {
                get { return _count; }
            }

            public IEnumerator<Size> GetEnumerator()
            {
                ImageInfo info = _dec.GetImageInfo(_stm);
                _count = info.FrameCount;

                MultiFramedImageDecoder mpdec = _dec as MultiFramedImageDecoder;
                if (mpdec != null)
                {
                    double timeoutSeconds = (_timeoutPages) ? 2 : 300;
                    DateTime timeout = DateTime.Now.AddSeconds(timeoutSeconds);

                    for (int i = 0; i < _count && timeout > DateTime.Now; i++)
                    {
                        _stm.Seek(0, SeekOrigin.Begin);
                        ImageInfo pginfo = mpdec.GetImageInfo(_stm, i);
                        yield return pginfo.Size;
                    }
                }
                else
                {
                    yield return info.Size;
                }
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        #endregion
    }
    #endregion
}
