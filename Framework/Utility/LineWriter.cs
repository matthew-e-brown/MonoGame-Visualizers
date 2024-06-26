namespace TrentCOIS.Tools.Visualization.Utility;

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// A <see cref="TextWriter"/> that, writes its output into an internal <see cref="StringBuilder"/>, just like
/// like <see cref="StringWriter"/> does, but caps itself to a specific number of lines.
/// </summary>
/// <remarks>
/// The main purpose of this class is to serve as a writer through which to redirect <see cref="Console.Out"/> and/or
/// <see cref="Console.Error"/>.
/// </remarks>
public class LineWriter : TextWriter
{
    // Yoinked from here:
    // https://github.com/microsoft/referencesource/blob/51cf7850defa8a17d815b4700b67116e3fa283c2/mscorlib/system/io/stringwriter.cs#L79-L86
    private readonly Encoding _encoding = new UnicodeEncoding(false, false);

    /// <summary>
    /// Returns the character encoding in which output is written.
    /// </summary>
    /// <remarks>
    /// This writer writes strings directly, which are stored as Unicode. So, this writer's encoding is always
    /// <see cref="UnicodeEncoding(bool, bool)"/>.
    /// </remarks>
    public override Encoding Encoding => _encoding;

    // --------

    private readonly StringBuilder builder; // where all the actual text goes
    private readonly Queue<int> lengths;    // how long each line in the builder is
    private int lineStart;                  // the index at which the next/currently-being-constructed line starts
    private int lineLimit;                  // maximum number of lines.
    private string? cached;                 // cached version of text, since the getter may be called up to 60 fps

    /// <summary>
    /// Crates a new <see cref="LineWriter"/>.
    /// </summary>
    public LineWriter() : this(null)
    { }

    /// <summary>
    /// Creates a new <see cref="LineWriter"/> using the specified <see cref="IFormatProvider"/>.
    /// </summary>
    /// <param name="formatProvider">The format provided to use for formatted writes to this writer.</param>
    public LineWriter(IFormatProvider? formatProvider) : base(formatProvider)
    {
        builder = new StringBuilder();
        lengths = new Queue<int>();
        lineLimit = int.MaxValue;
        lineStart = 0;
    }

    /// <summary>
    /// Gets or sets number of lines this <see cref="LineWriter"/> will hold until it begins removing old lines.
    /// </summary>
    public int LineLimit
    {
        get => lineLimit;
        set
        {
            if (value < lineLimit)
            {
                cached = null;
                RemoveExcessLines();
            }

            lineLimit = value;
        }
    }

    /// <summary>
    /// The full text of this <see cref="LineWriter"/>'s internal <see cref="StringBuilder"/>, excluding the final
    /// new-line.
    /// </summary>
    public string Text => cached ??= ToString();

    /// <summary>
    /// An iterator of all of the lines of this <see cref="LineWriter"/>, excluding new-lines on each one.
    /// </summary>
    public IEnumerable<string> Lines
    {
        get
        {
            int idx = 0;
            foreach (int len in lengths)
            {
                yield return builder.ToString(idx, len - 1); // Exclude \n
                idx += len;
            }

            int toEnd = builder.Length - idx - 1; // excluding \n
            if (toEnd > 0) yield return builder.ToString(idx, toEnd);
        }
    }

    private void FinalizeLine()
    {
        // A \n is now in the last spot in the buffer. Just in case, we get rid of any potential \r to make some math
        // easier elsewhere.
        int r = builder.Length - 2;
        if (builder[r] == '\r') builder.Remove(r, 1);

        int newLen = builder.Length - lineStart;    // end - start = length of new line
        lineStart = builder.Length;                 // keep track of where the next line will start
        lengths.Enqueue(newLen);                    // save this new line's length
        RemoveExcessLines();                        // remove any excess
    }

    private void RemoveExcessLines()
    {
        while (lengths.Count > LineLimit)
        {
            int amount = lengths.Dequeue();         // pop off the oldest line's length
            builder.Remove(0, amount);              // remove that many chars from the builder
            lineStart -= amount;                    // newest line starts that many chars closer to zero
        }
    }

    /// <inheritdoc/>
    public override void Write(char value)
    {
        builder.Append(value);
        cached = null;
        if (value == '\n') FinalizeLine();
    }

    /// <inheritdoc/>
    public override void Write(char[] buffer, int index, int count)
    {
        // StringWriter's overload of this does too, so may as well for consistency
        ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "Must be non-negative.");
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "Must be non-negative.");
        if (buffer.Length - index < count) throw new ArgumentException("Invalid offset + length into buffer.");

        builder.EnsureCapacity(builder.Length + count);
        base.Write(buffer, index, count); // will call char overload
    }

    /// <inheritdoc/>
    public override void Write(string? value)
    {
        if (value != null) builder.EnsureCapacity(builder.Length + value.Length);
        base.Write(value); // will call char overload
    }

    /// <summary>
    /// Converts this <see cref="LineWriter"/>'s internal <see cref="StringBuilder"/> into a string.
    /// </summary>
    /// <returns>
    /// The full contents of this writer's internal <see cref="StringBuilder"/>, excluding the final newline (if
    /// applicable).
    /// </returns>
    public override string ToString()
    {
        // If the builder currently ends with \n, remove that from the string version
        int len = builder.Length;
        if (len >= 1 && builder[len - 1] == '\n')
        {
            if (len >= 2 && builder[len - 2] == '\r') len -= 2;
            else len -= 1;
        }

        return builder.ToString(0, len);
    }
}
