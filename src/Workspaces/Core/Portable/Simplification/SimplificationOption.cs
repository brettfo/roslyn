﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Options;

namespace Microsoft.CodeAnalysis.Simplification
{
    /// <summary>
    /// This Object contains the options that needs to be drilled down to the Simplification Engine
    /// </summary>
    public static class SimplificationOptions
    {
        internal const string NonPerLanguageFeatureName = "Simplification";
        internal const string PerLanguageFeatureName = "SimplificationPerLanguage";

        /// <summary>
        /// This option tells the simplification engine if the Qualified Name should be replaced by Alias
        /// if the user had initially not used the Alias
        /// </summary>
        public static Option<bool> PreferAliasToQualification { get; } = new Option<bool>(NonPerLanguageFeatureName, "PreferAliasToQualification", true);

        /// <summary>
        /// This option influences the name reduction of members of a module in VB. If set to true, the 
        /// name reducer will e.g. reduce Namespace.Module.Member to Namespace.Member.
        /// </summary>
        public static Option<bool> PreferOmittingModuleNamesInQualification { get; } = new Option<bool>(NonPerLanguageFeatureName, "PreferOmittingModuleNamesInQualification", true);

        /// <summary>
        /// This option says that if we should simplify the Generic Name which has the type argument inferred
        /// </summary>
        public static Option<bool> PreferImplicitTypeInference { get; } = new Option<bool>(NonPerLanguageFeatureName, "PreferImplicitTypeInference", true);

        /// <summary>
        /// This option says if we should simplify the Explicit Type in Local Declarations
        /// </summary>
        public static Option<bool> PreferImplicitTypeInLocalDeclaration { get; } = new Option<bool>(NonPerLanguageFeatureName, "PreferImplicitTypeInLocalDeclaration", false);

        /// <summary>
        /// This option says if we should simplify to NonGeneric Name rather than GenericName
        /// </summary>
        public static Option<bool> AllowSimplificationToGenericType { get; } = new Option<bool>(NonPerLanguageFeatureName, "AllowSimplificationToGenericType", false);

        /// <summary>
        /// This option says if we should simplify from Derived types to Base types in Static Member Accesses
        /// </summary>
        public static Option<bool> AllowSimplificationToBaseType { get; } = new Option<bool>(NonPerLanguageFeatureName, "AllowSimplificationToBaseType", true);

        /// <summary>
        /// This option says if we should simplify away the <see langword="this"/>. or <see langword="Me"/>. in member access expression
        /// </summary>
        public static PerLanguageOption<bool> QualifyMemberAccessWithThisOrMe { get; } = new PerLanguageOption<bool>(PerLanguageFeatureName, "QualifyMemberAccessWithThisOrMe", defaultValue: true);

        /// <summary>
        /// This option says if we should simplify away the <see langword="this"/>. or <see langword="Me"/>. in member field access expressions.
        /// </summary>
        public static PerLanguageOption<bool> QualifyMemberFieldAccessWithThisOrMe { get; } = new PerLanguageOption<bool>(PerLanguageFeatureName, "QualifyMemberFieldAccessWithThisOrMe", defaultValue: true);

        /// <summary>
        /// This option says if we should simplify away the <see langword="this"/>. or <see langword="Me"/>. in member property access expressions.
        /// </summary>
        public static PerLanguageOption<bool> QualifyMemberPropertyAccessWithThisOrMe { get; } = new PerLanguageOption<bool>(PerLanguageFeatureName, "QualifyMemberPropertyAccessWithThisOrMe", defaultValue: false);

        /// <summary>
        /// This option says if we should simplify away the <see langword="this"/>. or <see langword="Me"/>. in member method access expressions.
        /// </summary>
        public static PerLanguageOption<bool> QualifyMemberMethodAccessWithThisOrMe { get; } = new PerLanguageOption<bool>(PerLanguageFeatureName, "QualifyMemberMethodAccessWithThisOrMe", defaultValue: false);

        /// <summary>
        /// This option says if we should simplify away the <see langword="this"/>. or <see langword="Me"/>. in member event access expressions.
        /// </summary>
        public static PerLanguageOption<bool> QualifyMemberEventAccessWithThisOrMe { get; } = new PerLanguageOption<bool>(PerLanguageFeatureName, "QualifyMemberEventAccessWithThisOrMe", defaultValue: false);

        /// <summary>
        /// This option says if we should prefer keyword for Intrinsic Predefined Types in Declarations
        /// </summary>
        public static PerLanguageOption<bool> PreferIntrinsicPredefinedTypeKeywordInDeclaration { get; } = new PerLanguageOption<bool>(PerLanguageFeatureName, "PreferIntrinsicPredefinedTypeKeywordInDeclaration", defaultValue: true);

        /// <summary>
        /// This option says if we should prefer keyword for Intrinsic Predefined Types in Member Access Expression
        /// </summary>
        public static PerLanguageOption<bool> PreferIntrinsicPredefinedTypeKeywordInMemberAccess { get; } = new PerLanguageOption<bool>(PerLanguageFeatureName, "PreferIntrinsicPredefinedTypeKeywordInMemberAccess", defaultValue: true);
    }
}
