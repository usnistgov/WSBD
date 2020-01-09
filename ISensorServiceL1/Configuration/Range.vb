Option Infer On

Imports System.Runtime.Serialization

Namespace Nist.Bcl.Wsbd

    <DataContract(name:="Range", Namespace:=Constants.WsbdNamespace)>
    Public Class Range
        Private mMinimum As Object
        <DataMember(EmitDefaultValue:=False, Name:="minimum")> _
        Public Property Minimum As Object
            Get
                Return mMinimum
            End Get
            Set(ByVal value As Object)
                If mMinimum IsNot Nothing Then Throw New WsbdRangesAreImmutableException
                'If ContainsValidValue() Then Throw New WsbdRangesAreImmutableException
                ValidateType(value.GetType())
                mMinimum = value
            End Set
        End Property

        Private mMaximum As Object
        <DataMember(EmitDefaultValue:=False, Name:="maximum")> _
        Public Property Maximum As Object
            Get
                Return mMaximum
            End Get
            Set(ByVal value As Object)
                If mMaximum IsNot Nothing Then Throw New WsbdRangesAreImmutableException
                'If ContainsValidValue() Then Throw New WsbdRangesAreImmutableException
                ValidateType(value.GetType())
                mMaximum = value
            End Set
        End Property

        <DataMember(EmitDefaultValue:=False, Name:="minimumIsExclusive")> _
        Public Property MinimumIsExclusive As Boolean?

        <DataMember(EmitDefaultValue:=False, Name:="maximumIsExclusive")> _
        Public Property MaximumIsExclusive As Boolean?

        Private Sub ValidateType(ByVal t As Type)

            ' Upon the first type validation, we cache the datatype and
            ' if the type is comparable or not. This cache is extremely
            ' effective in speeding up working with WsbdRanges

            If mContentType Is Nothing Then
                mContentType = t
                mContentTypeImplementsIComparable = t.GetInterfaces.Contains(GetType(IComparable))
                mContentIsWholeNumber = IsWholeNumber(t)
            Else
                If Not mContentType.Equals(t) Then
                    Throw New WsbdRangeTypeMismatchException
                End If
            End If
        End Sub

        Private mContentType As Type
        Private mContentTypeImplementsIComparable As Boolean
        Private mContentIsWholeNumber As Boolean
        Public ReadOnly Property ContentType As Type
            Get
                Return mContentType
            End Get
        End Property

        Private Function IsWholeNumber(ByVal t As Type)
            Return _
                GetType(Byte).Equals(t) OrElse
                GetType(Short).Equals(t) OrElse
                GetType(Integer).Equals(t) OrElse
                GetType(Long).Equals(t) OrElse
                GetType(SByte).Equals(t) OrElse
                GetType(UShort).Equals(t) OrElse
                GetType(UInteger).Equals(t) OrElse
                GetType(ULong).Equals(t)
        End Function

    End Class



    <Serializable()> Public MustInherit Class WsbdRangeException
        Inherits Exception
        Public Sub New()

        End Sub
        Public Sub New(ByVal message As String)
            MyBase.New(message)

        End Sub
        Public Sub New(ByVal message As String, ByVal innerException As Exception)
            MyBase.New(message, innerException)

        End Sub
        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
            MyBase.New(info, context)

        End Sub
    End Class

    <Serializable()> Public Class WsbdRangesAreImmutableException
        Inherits WsbdRangeException
        Public Sub New()

        End Sub
        Public Sub New(ByVal message As String)
            MyBase.New(message)

        End Sub
        Public Sub New(ByVal message As String, ByVal innerException As Exception)
            MyBase.New(message, innerException)

        End Sub
        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
            MyBase.New(info, context)

        End Sub
    End Class

End Namespace