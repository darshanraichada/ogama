// <copyright file="VGImage.cs" company="FU Berlin">
// ******************************************************
// OGAMA - open gaze and mouse analyzer 
// Copyright (C) 2010 Adrian Vo�k�hler  
// ------------------------------------------------------------------------
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation; either version 2 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program; if not, write to the Free Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// **************************************************************
// </copyright>
// <author>Adrian Vo�k�hler</author>
// <email>adrian.vosskuehler@fu-berlin.de</email>

namespace VectorGraphics.Elements
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Drawing;
  using System.Drawing.Drawing2D;
  using System.IO;
  using System.Text;
  using System.Windows.Forms;
  using System.Xml.Serialization;
  using VectorGraphics.Tools;

  /// <summary>
  /// Inherited from <see cref="VGElement"/>. 
  /// A serializable class that represents a vector graphics image.
  /// </summary>
  [Serializable]
  public class VGImage : VGElement
  {
    ///////////////////////////////////////////////////////////////////////////////
    // Defining Constants                                                        //
    ///////////////////////////////////////////////////////////////////////////////
    #region CONSTANTS
    #endregion //CONSTANTS

    ///////////////////////////////////////////////////////////////////////////////
    // Defining Variables, Enumerations, Events                                  //
    ///////////////////////////////////////////////////////////////////////////////
    #region FIELDS

    /// <summary>
    /// Saves the filename without path of the image.
    /// </summary>
    private string filename;

    /// <summary>
    /// Saves the path to the file for this image.
    /// </summary>
    private string path;

    /// <summary>
    /// Saves the current image.
    /// </summary>
    private Image image;

    /// <summary>
    /// Saves the <see cref="ImageLayout"/> for this image.
    /// </summary>
    private ImageLayout layout;

    /// <summary>
    /// Saves the size to canvas that holds this image.
    /// </summary>
    private Size canvas;

    #endregion //FIELDS

    ///////////////////////////////////////////////////////////////////////////////
    // Construction and Initializing methods                                     //
    ///////////////////////////////////////////////////////////////////////////////
    #region CONSTRUCTION

    /// <summary>
    /// Initializes a new instance of the VGImage class.
    /// When newShapeDrawAction is set to None, only the image will
    /// be drawn, with Edge an additional border is drawn,
    /// with fill an additional (hopefully transparent) fill is drawn over the image.
    /// </summary>
    /// <param name="newShapeDrawAction">Drawing action: Edge, Fill, Both, None</param>
    /// <param name="newPen">Pen for additional borderline.</param>
    /// <param name="newBrush">Brush for additional fills.</param>
    /// <param name="newFont">Font for drawing name</param>
    /// <param name="newFontColor">Font color for drawing name.</param>
    /// <param name="newImageFile">filename without path</param>
    /// <param name="newPath">path to image file</param>
    /// <param name="newLayout"><see cref="ImageLayout"/> of the image</param>
    /// <param name="newCanvas"><see cref="Size"/> of the owning original canvas</param>
    /// <param name="newStyleGroup">Group Enumeration, <see cref="VGStyleGroup"/></param>
    /// <param name="newName">Name of Element</param>
    /// <param name="newElementGroup">Element group description</param>
    public VGImage(
      ShapeDrawAction newShapeDrawAction,
      Pen newPen,
      Brush newBrush,
      Font newFont,
      Color newFontColor,
      string newImageFile,
      string newPath,
      ImageLayout newLayout,
      Size newCanvas,
      VGStyleGroup newStyleGroup,
      string newName,
      string newElementGroup)
      : base(
      newShapeDrawAction,
      newPen,
      newBrush,
      newFont,
      newFontColor,
      RectangleF.Empty,
      newStyleGroup,
      newName,
      newElementGroup,
      null)
    {
      this.filename = newImageFile;
      this.path = newPath;
      this.canvas = newCanvas;

      if (!this.CreateInternalImage())
      {
        return;
      }

      GraphicsUnit unit = new GraphicsUnit();
      this.Bounds = this.image.GetBounds(ref unit);
      this.layout = newLayout;
    }

    /// <summary>
    /// Initializes a new instance of the VGImage class.
    /// Creates a new VGImage with the given image and properties,
    /// that is not persistent, because it is not saved to disk.
    /// </summary>
    /// <param name="newImage">An <see cref="Image"/> that should be displayed.</param>
    /// <param name="newLayout">An <see cref="ImageLayout"/> for the image.</param>
    /// <param name="newCanvas"><see cref="Size"/> of the owning original canvas</param>
    public VGImage(Image newImage, ImageLayout newLayout, Size newCanvas)
      : base(ShapeDrawAction.None, Pens.Red)
    {
      this.path = string.Empty;
      this.filename = string.Empty;
      this.layout = newLayout;
      this.canvas = newCanvas;
      this.image = (Image)newImage.Clone();
      GraphicsUnit unit = new GraphicsUnit();
      this.Bounds = this.image.GetBounds(ref unit);
    }

    /// <summary>
    /// Prevents a default instance of the VGImage class from being created.
    /// Parameterless constructor. Used for serialization.
    /// </summary>
    private VGImage()
    {
      this.path = string.Empty;
      this.filename = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the VGImage class.
    /// Clone Constructor. Creates new <see cref="VGImage"/> that is
    /// identical to the given <see cref="VGImage"/>.
    /// </summary>
    /// <param name="cloneImage"><see cref="VGImage"/> to clone</param>
    private VGImage(VGImage cloneImage)
      : base(
      cloneImage.ShapeDrawAction,
      cloneImage.Pen,
      cloneImage.Brush,
      cloneImage.Font,
      cloneImage.FontColor,
      cloneImage.Bounds,
      cloneImage.StyleGroup,
      cloneImage.Name,
      cloneImage.ElementGroup,
      cloneImage.Sound)
    {
      this.filename = cloneImage.Filename;
      this.path = cloneImage.Filepath;

      // Removed because when starting recording that leaded
      // to an cross thread problem (object is in use elsewhere)
      // _image = image.StimulusImage;
      this.layout = cloneImage.Layout;
      this.canvas = cloneImage.Canvas;
    }

    #endregion //CONSTRUCTION

    ///////////////////////////////////////////////////////////////////////////////
    // Defining Properties                                                       //
    ///////////////////////////////////////////////////////////////////////////////
    #region PROPERTIES

    /// <summary>
    /// Gets or sets the filename without path for this image.
    /// </summary>
    /// <remarks>This property is used for reloading the image from file.</remarks>
    /// <value>A <see cref="string"/> with the images filename</value>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public string Filename
    {
      get { return this.filename; }
      set { this.filename = value; }
    }

    /// <summary>
    /// Gets or sets the filenames path for this image.
    /// </summary>
    /// <remarks>This property is used for reloading the image from file.</remarks>
    /// <value>A <see cref="string"/> with the images path.</value>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    [XmlIgnore()]
    public string Filepath
    {
      get { return this.path; }
      set { this.path = value; }
    }

    /// <summary>
    /// Gets the image filename with path.
    /// </summary>
    [XmlIgnore()]
    public string FullFilename
    {
      get
      {
        if (this.filename == string.Empty || this.filename == null)
        {
          throw new ArgumentNullException("Image filename is empty, so it could no be loaded");
        }

        if (this.path == null || this.path == string.Empty)
        {
          string newPath = Path.GetDirectoryName(this.filename);
          if (newPath != string.Empty)
          {
            this.path = newPath;
            this.filename = Path.GetFileName(this.filename);
          }
          else
          {
          }
        }

        return Path.Combine(this.path, this.filename);
      }
    }

    /// <summary>
    /// Gets or sets the size of the canvas on which the image is drawn.
    /// </summary>
    /// <value>A <see cref="Size"/> with the width and height
    /// of the canvas on which the image should be drawn.</value>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public Size Canvas
    {
      get { return this.canvas; }
      set { this.canvas = value; }
    }

    /// <summary>
    /// Gets or sets the image to draw.
    /// </summary>
    /// <remarks>When this is null (after deserialization) the image is
    /// reconstructed from file.</remarks>
    /// <value>A <see cref="Image"/> with the image.</value>
    [XmlIgnoreAttribute()]
    [Category("Content")]
    [Description("The image to use as stimulus.")]
    public Image StimulusImage
    {
      get
      {
        if (this.image == null)
        {
          this.CreateInternalImage();
        }

        return this.image;
      }

      set
      {
        this.image = value;
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="ImageLayout"/> of this image.
    /// </summary>
    /// <value>An <see cref="ImageLayout"/> enumeration member.</value>
    [Category("Layout")]
    [Description("The image layout.")]
    public ImageLayout Layout
    {
      get { return this.layout; }
      set { this.layout = value; }
    }

    #endregion //PROPERTIES

    ///////////////////////////////////////////////////////////////////////////////
    // Public methods                                                            //
    ///////////////////////////////////////////////////////////////////////////////
    #region PUBLICMETHODS

    /// <summary>
    /// Checks if the bitmap in the stimulus <see cref="Filename"/> property
    /// exists and creates the internal <see cref="StimulusImage"/>
    /// </summary>
    /// <returns><strong>True</strong> if creation succeeded, 
    /// otherwise <strong>false</strong>.</returns>
    public bool CreateInternalImage()
    {
      string fullFileName = this.FullFilename;

      if (!File.Exists(fullFileName))
      {
        if (this.Size.IsEmpty)
        {
          this.Size = this.canvas;
        }

        this.image = Images.CreateNotFoundImage(this.Size.ToSize(), this.Filename);
      }
      else
      {
        // This is the method not to keep the file open,
        // but create a copy in memory from the file 
        this.image = Images.GetImageOfFile(fullFileName);
      }

      return true;
    }

    #endregion //PUBLICMETHODS

    ///////////////////////////////////////////////////////////////////////////////
    // Inherited methods                                                         //
    ///////////////////////////////////////////////////////////////////////////////
    #region OVERRIDES

    /// <summary>
    /// Overridden <see cref="VGElement.Draw(Graphics)"/>.  
    /// Draws the image to the given graphics context.
    /// </summary>
    /// <param name="graphics">Graphics context to draw to</param>
    /// <exception cref="ArgumentNullException">Thrown, when graphics object is null.</exception>
    public override void Draw(Graphics graphics)
    {
      if (graphics == null)
      {
        throw new ArgumentNullException("Graphics object should not be null.");
      }

      if (this.image == null)
      {
        if (!this.CreateInternalImage())
        {
          return;
        }
      }

      Rectangle drawingCanvas = new Rectangle(0, 0, this.canvas.Width, this.canvas.Height);
      RectangleF drawing_rectangle = drawingCanvas;

      if (this.ShapeDrawAction == (this.ShapeDrawAction | ShapeDrawAction.Edge))
      {
        drawing_rectangle.Inflate(-this.Pen.Width, -this.Pen.Width);
      }

      switch (this.layout)
      {
        case ImageLayout.Tile:
          using (TextureBrush b = new TextureBrush(this.image, WrapMode.Tile))
          {
            graphics.FillRectangle(b, drawing_rectangle);
          }

          break;
        case ImageLayout.Center:
          drawing_rectangle.Location = new Point(
            (int)(drawingCanvas.Width / 2 - this.image.Width / 2),
            (int)(drawingCanvas.Height / 2 - this.image.Height / 2));

          drawing_rectangle.Size = this.image.Size;
          graphics.DrawImage(this.image, drawing_rectangle);
          break;
        case ImageLayout.None:
          drawing_rectangle.Location = this.Location;
          if (this.ModifierKeys == Keys.Control)
          {
            drawing_rectangle.Size = this.image.Size;
          }
          else if (base.ModifierKeys == Keys.Alt)
          {
            SizeF proportionalSize = new SizeF();
            proportionalSize.Width = base.Bounds.Width;
            proportionalSize.Height = this.image.Height / (float)this.image.Width * proportionalSize.Width;
            drawing_rectangle.Size = System.Drawing.Size.Round(proportionalSize);
          }
          else
          {
            drawing_rectangle.Size = base.Bounds.Size;
          }

          if (this.ShapeDrawAction == (this.ShapeDrawAction | ShapeDrawAction.Edge))
          {
            drawing_rectangle.Inflate(-this.Pen.Width, -this.Pen.Width);
          }

          graphics.DrawImage(this.image, drawing_rectangle);
          break;
        case ImageLayout.Stretch:
          graphics.DrawImage(this.image, drawing_rectangle);
          break;
        case ImageLayout.Zoom:
          if ((float)this.image.Width / (float)this.image.Height < (float)drawing_rectangle.Width / (float)drawing_rectangle.Height)
          {
            drawing_rectangle.Width = this.image.Width * ((float)drawing_rectangle.Height / (float)this.image.Height);
            drawing_rectangle.X = (drawingCanvas.Width - drawing_rectangle.Width) / 2;
          }
          else
          {
            drawing_rectangle.Height = this.image.Height * ((float)drawing_rectangle.Width / (float)this.image.Width);
            drawing_rectangle.Y = (drawingCanvas.Height - drawing_rectangle.Height) / 2;
          }

          if (this.ShapeDrawAction == (this.ShapeDrawAction | ShapeDrawAction.Edge))
          {
            drawing_rectangle.Inflate(-this.Pen.Width, -this.Pen.Width);
          }

          graphics.DrawImage(this.image, drawing_rectangle);
          break;
      }

      if (this.ShapeDrawAction == (this.ShapeDrawAction | ShapeDrawAction.Fill))
      {
        graphics.FillRectangle(this.Brush, drawing_rectangle);
      }

      if (this.ShapeDrawAction == (this.ShapeDrawAction | ShapeDrawAction.Edge))
      {
        RectangleF halfInlineRect = drawing_rectangle;
        halfInlineRect.Inflate(this.Pen.Width / 2, this.Pen.Width / 2);
        graphics.DrawRectangle(this.Pen, halfInlineRect.X, halfInlineRect.Y, halfInlineRect.Width, halfInlineRect.Height);
      }

      if (this.ShapeDrawAction == (this.ShapeDrawAction | ShapeDrawAction.Edge))
      {
        RectangleF boundingRect = drawing_rectangle;
        boundingRect.Inflate(this.Pen.Width, this.Pen.Width);

        // When drawing style has changed we shall reset the bounds according to new bounds.
        this.Bounds = boundingRect;
      }
      else
      {
        // When drawing style has changed we shall reset the bounds according to new bounds.
        this.Bounds = drawing_rectangle;
      }

      // Draw name and selection frame if applicable
      base.Draw(graphics);
    }

    /// <summary>
    /// Overridden <see cref="VGElement.Reset()"/>. 
    /// Reset the current image element to
    /// default values.
    /// </summary>
    public override void Reset()
    {
      base.Reset();
      this.canvas = new Size(100, 100);
      this.image = null;
      this.layout = ImageLayout.Center;
      this.filename = string.Empty;
    }

    /// <summary>
    /// Overridden <see cref="Object.ToString()"/> method.
    /// Returns the <see cref="VGImage"/> properties as a human readable string.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this <see cref="VGImage"/>.</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("VGImage, File: ");
      sb.Append(Path.GetFileName(this.filename));
      sb.Append(" ; ImageLayout: ");
      sb.Append(this.layout.ToString());
      sb.Append(" ; Bounds: ");
      sb.Append(this.Bounds.ToString());
      return sb.ToString();
    }

    /// <summary>
    /// Overridden <see cref="VGElement.ToShortString()"/> method.
    /// Returns the main <see cref="VGImage"/> properties as a human readable string.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the 
    /// current <see cref="VGImage"/> in short form with its main properties.</returns>
    public override string ToShortString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("Image: ");
      string text = Path.GetFileName(this.filename);
      sb.Append(text.Substring(0, text.Length > 12 ? Math.Max(12, text.Length - 1) : text.Length));
      sb.Append(" ...");
      return sb.ToString();
    }

    /// <summary>
    /// Releases the resources used by the element.
    /// </summary>
    public override void Dispose()
    {
      base.Dispose();
      if (this.image != null)
      {
        this.image.Dispose();
        this.image = null;
      }
    }

    /// <summary>
    /// Overridden <see cref="VGElement.CloneCore()"/>.  
    /// Creates a excact copy of given <see cref="VGImage"/>
    /// </summary>
    /// <returns>Excact copy of this <see cref="VGImage"/></returns>
    protected override VGElement CloneCore()
    {
      return new VGImage(this);
    }

    /// <summary>
    /// Overridden. This resets the layout to <see cref="ImageLayout.None"/>,
    /// because the image is going to be moved.
    /// </summary>
    /// <param name="translationMatrix">Translation Matrix, that performs the translation.</param>
    protected override void NewPosition(Matrix translationMatrix)
    {
      this.Layout = ImageLayout.None;
      base.NewPosition(translationMatrix);
    }

    #endregion //OVERRIDES

    ///////////////////////////////////////////////////////////////////////////////
    // Eventhandler                                                              //
    ///////////////////////////////////////////////////////////////////////////////
    #region EVENTS

    ///////////////////////////////////////////////////////////////////////////////
    // Eventhandler for UI, Menu, Buttons, Toolbars etc.                         //
    ///////////////////////////////////////////////////////////////////////////////
    #region WINDOWSEVENTHANDLER
    #endregion //WINDOWSEVENTHANDLER

    ///////////////////////////////////////////////////////////////////////////////
    // Eventhandler for Custom Defined Events                                    //
    ///////////////////////////////////////////////////////////////////////////////
    #region CUSTOMEVENTHANDLER
    #endregion //CUSTOMEVENTHANDLER

    #endregion //EVENTS

    ///////////////////////////////////////////////////////////////////////////////
    // Methods and Eventhandling for Background tasks                            //
    ///////////////////////////////////////////////////////////////////////////////
    #region BACKGROUNDWORKER
    #endregion //BACKGROUNDWORKER

    ///////////////////////////////////////////////////////////////////////////////
    // Methods for doing main class job                                          //
    ///////////////////////////////////////////////////////////////////////////////
    #region METHODS
    #endregion //METHODS

    ///////////////////////////////////////////////////////////////////////////////
    // Small helping Methods                                                     //
    ///////////////////////////////////////////////////////////////////////////////
    #region HELPER
    #endregion //HELPER
  }
}
