﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.1
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System

Namespace My.Resources
    
    'This class was auto-generated by the StronglyTypedResourceBuilder
    'class via a tool like ResGen or Visual Studio.
    'To add or remove a member, edit your .ResX file then rerun ResGen
    'with the /str option, or rebuild your VS project.
    '''<summary>
    '''  A strongly-typed resource class, for looking up localized strings, etc.
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute(),  _
     Global.Microsoft.VisualBasic.HideModuleNameAttribute()>  _
    Friend Module Resources
        
        Private resourceMan As Global.System.Resources.ResourceManager
        
        Private resourceCulture As Global.System.Globalization.CultureInfo
        
        '''<summary>
        '''  Returns the cached ResourceManager instance used by this class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("Resources", GetType(Resources).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  Overrides the current thread's CurrentUICulture property for all
        '''  resource lookups using this strongly typed resource class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to The server could not generate a unique session id..
        '''</summary>
        Friend ReadOnly Property CouldNotGenerateUniqueSessionId() As String
            Get
                Return ResourceManager.GetString("CouldNotGenerateUniqueSessionId", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to The server could not parse &apos;{0}&apos; into a valid session id..
        '''</summary>
        Friend ReadOnly Property CouldNotParseSessionId() As String
            Get
                Return ResourceManager.GetString("CouldNotParseSessionId", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to The server could not parse &apos;{0}&apos; into a valid capture id..
        '''</summary>
        Friend ReadOnly Property CountNotParseCaptureId() As String
            Get
                Return ResourceManager.GetString("CountNotParseCaptureId", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to The data store already has data associated with id &apos;{0}&apos;..
        '''</summary>
        Friend ReadOnly Property DataAlreadyExistsForRecord() As String
            Get
                Return ResourceManager.GetString("DataAlreadyExistsForRecord", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to The data store record for id &apos;{0}&apos; is corrupt..
        '''</summary>
        Friend ReadOnly Property IncompleteDataStoreRecord() As String
            Get
                Return ResourceManager.GetString("IncompleteDataStoreRecord", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to The server cannot support any additional sessions..
        '''</summary>
        Friend ReadOnly Property MaximumConcurrentSessionsExceeded() As String
            Get
                Return ResourceManager.GetString("MaximumConcurrentSessionsExceeded", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to The server has no session registerd with id &apos;{0}&apos;..
        '''</summary>
        Friend ReadOnly Property SessionNotRegistered() As String
            Get
                Return ResourceManager.GetString("SessionNotRegistered", resourceCulture)
            End Get
        End Property
    End Module
End Namespace
