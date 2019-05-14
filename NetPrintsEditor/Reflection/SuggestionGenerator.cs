using System;
using System.Collections.Generic;
using System.Linq;
using NetPrints.Base;
using NetPrints.Core;
using NetPrints.Graph;
using NetPrintsEditor.Dialogs;

namespace NetPrintsEditor.Reflection
{
    public class SuggestionGenerator : ISuggestionGenerator
    {
        private readonly Dictionary<Type, List<object>> builtInNodes = new Dictionary<Type, List<object>>()
        {
            [typeof(MethodGraph)] = new List<object>()
            {
                TypeSpecifier.FromType<ForLoopNode>(),
                TypeSpecifier.FromType<IfElseNode>(),
                TypeSpecifier.FromType<ConstructorNode>(),
                TypeSpecifier.FromType<TypeOfNode>(),
                TypeSpecifier.FromType<ExplicitCastNode>(),
                TypeSpecifier.FromType<ReturnNode>(),
                TypeSpecifier.FromType<MakeArrayNode>(),
                TypeSpecifier.FromType<LiteralNode>(),
                TypeSpecifier.FromType<TypeNode>(),
                TypeSpecifier.FromType<MakeArrayTypeNode>(),
                TypeSpecifier.FromType<ThrowNode>(),
                TypeSpecifier.FromType<AwaitNode>(),
                TypeSpecifier.FromType<TernaryNode>(),
                TypeSpecifier.FromType<DefaultNode>(),
            },
            [typeof(ConstructorGraph)] = new List<object>()
            {
                TypeSpecifier.FromType<ForLoopNode>(),
                TypeSpecifier.FromType<IfElseNode>(),
                TypeSpecifier.FromType<ConstructorNode>(),
                TypeSpecifier.FromType<TypeOfNode>(),
                TypeSpecifier.FromType<ExplicitCastNode>(),
                TypeSpecifier.FromType<MakeArrayNode>(),
                TypeSpecifier.FromType<LiteralNode>(),
                TypeSpecifier.FromType<TypeNode>(),
                TypeSpecifier.FromType<MakeArrayTypeNode>(),
                TypeSpecifier.FromType<ThrowNode>(),
                TypeSpecifier.FromType<TernaryNode>(),
                TypeSpecifier.FromType<DefaultNode>(),
            },
            [typeof(ClassGraph)] = new List<object>()
            {
                TypeSpecifier.FromType<TypeNode>(),
                TypeSpecifier.FromType<MakeArrayTypeNode>(),
            },
        };

        private List<object> GetBuiltInNodes(NodeGraph graph)
        {
            if (builtInNodes.TryGetValue(graph.GetType(), out var nodes))
            {
                return nodes;
            }

            return new List<object>();
        }

        public IEnumerable<(string, object)> GetSuggestions(INodeGraph graph, INodePin pin = null)
        {
            // Show all relevant methods for the type of the pin
            IEnumerable<(string, object)> suggestions = new (string, object)[0];

            void AddSuggestionsWithCategory(string category, IEnumerable<object> newSuggestions)
            {
                suggestions = suggestions.Concat(newSuggestions.Select(suggestion => (category, suggestion)));
            }

            NodeGraph nodeGraph = (NodeGraph)graph;

            if (pin != null)
            {
                if (pin is NodeOutputDataPin odp)
                {
                    if (odp.PinType.Value is TypeSpecifier pinTypeSpec)
                    {
                        // Add make delegate
                        AddSuggestionsWithCategory("NetPrints", new[] { new MakeDelegateTypeInfo(pinTypeSpec, nodeGraph.Class.Type) });

                        // Add variables and methods of the pin type
                        AddSuggestionsWithCategory("Pin Variables",
                            App.ReflectionProvider.GetVariables(
                                new ReflectionProviderVariableQuery()
                                    .WithType(pinTypeSpec)
                                    .WithVisibleFrom(nodeGraph.Class.Type)
                                    .WithStatic(false)));

                        AddSuggestionsWithCategory("Pin Methods", App.ReflectionProvider.GetMethods(
                            new ReflectionProviderMethodQuery()
                                .WithVisibleFrom(nodeGraph.Class.Type)
                                .WithStatic(false)
                                .WithType(pinTypeSpec)));

                        // Add methods of the base types that can accept the pin type as argument
                        foreach (var baseType in nodeGraph.Class.AllBaseTypes)
                        {
                            AddSuggestionsWithCategory("This Methods", App.ReflectionProvider.GetMethods(
                                new ReflectionProviderMethodQuery()
                                    .WithVisibleFrom(nodeGraph.Class.Type)
                                    .WithStatic(false)
                                    .WithArgumentType(pinTypeSpec)
                                    .WithType(baseType)));
                        }

                        // Add static functions taking the type of the pin
                        AddSuggestionsWithCategory("Static Methods", App.ReflectionProvider.GetMethods(
                            new ReflectionProviderMethodQuery()
                                .WithArgumentType(pinTypeSpec)
                                .WithVisibleFrom(nodeGraph.Class.Type)
                                .WithStatic(true)));
                    }
                }
                else if (pin is NodeInputDataPin idp)
                {
                    if (idp.PinType.Value is TypeSpecifier pinTypeSpec)
                    {
                        // Variables of base classes that inherit from needed type
                        foreach (var baseType in nodeGraph.Class.AllBaseTypes)
                        {
                            AddSuggestionsWithCategory("This Variables", App.ReflectionProvider.GetVariables(
                                new ReflectionProviderVariableQuery()
                                    .WithType(baseType)
                                    .WithVisibleFrom(nodeGraph.Class.Type)
                                    .WithVariableType(pinTypeSpec, true)));
                        }

                        // Add static functions returning the type of the pin
                        AddSuggestionsWithCategory("Static Methods", App.ReflectionProvider.GetMethods(
                            new ReflectionProviderMethodQuery()
                                .WithStatic(true)
                                .WithVisibleFrom(nodeGraph.Class.Type)
                                .WithReturnType(pinTypeSpec)));
                    }
                }
                else if (pin is NodeOutputExecPin oxp)
                {
                    GraphUtil.DisconnectPin(oxp);

                    AddSuggestionsWithCategory("NetPrints", GetBuiltInNodes(nodeGraph));

                    foreach (var baseType in nodeGraph.Class.AllBaseTypes)
                    {
                        AddSuggestionsWithCategory("This Methods", App.ReflectionProvider.GetMethods(
                            new ReflectionProviderMethodQuery()
                                .WithType(baseType)
                                .WithStatic(false)
                                .WithVisibleFrom(nodeGraph.Class.Type)));
                    }

                    AddSuggestionsWithCategory("Static Methods", App.ReflectionProvider.GetMethods(
                        new ReflectionProviderMethodQuery()
                            .WithStatic(true)
                            .WithVisibleFrom(nodeGraph.Class.Type)));
                }
                else if (pin is NodeInputExecPin ixp)
                {
                    AddSuggestionsWithCategory("NetPrints", GetBuiltInNodes(nodeGraph));

                    foreach (var baseType in nodeGraph.Class.AllBaseTypes)
                    {
                        AddSuggestionsWithCategory("This Methods", App.ReflectionProvider.GetMethods(
                        new ReflectionProviderMethodQuery()
                            .WithType(baseType)
                            .WithStatic(false)
                            .WithVisibleFrom(nodeGraph.Class.Type)));
                    }

                    AddSuggestionsWithCategory("Static Methods", App.ReflectionProvider.GetMethods(
                        new ReflectionProviderMethodQuery()
                            .WithStatic(true)
                            .WithVisibleFrom(nodeGraph.Class.Type)));
                }
                else if (pin is NodeInputTypePin itp)
                {
                    // TODO: Consider static types
                    AddSuggestionsWithCategory("Types", App.ReflectionProvider.GetNonStaticTypes());
                }
                else if (pin is NodeOutputTypePin otp)
                {
                    if (nodeGraph is ExecutionGraph && otp.InferredType.Value is TypeSpecifier typeSpecifier)
                    {
                        AddSuggestionsWithCategory("Pin Static Methods", App.ReflectionProvider
                            .GetMethods(new ReflectionProviderMethodQuery()
                                .WithType(typeSpecifier)
                                .WithStatic(true)
                                .WithVisibleFrom(nodeGraph.Class.Type)));
                    }

                    // Types with type parameters
                    AddSuggestionsWithCategory("Generic Types", App.ReflectionProvider.GetNonStaticTypes()
                        .Where(t => t.GenericArguments.Any()));

                    if (nodeGraph is ExecutionGraph)
                    {
                        // Public static methods that have type parameters
                        AddSuggestionsWithCategory("Generic Static Methods", App.ReflectionProvider
                            .GetMethods(new ReflectionProviderMethodQuery()
                                .WithStatic(true)
                                .WithHasGenericArguments(true)
                                .WithVisibleFrom(nodeGraph.Class.Type)));
                    }
                }
            }
            else
            {
                AddSuggestionsWithCategory("NetPrints", GetBuiltInNodes(nodeGraph));

                if (nodeGraph is ExecutionGraph)
                {
                    // Get properties and methods of base class.
                    foreach (var baseType in nodeGraph.Class.AllBaseTypes)
                    {
                        AddSuggestionsWithCategory("This Variables", App.ReflectionProvider.GetVariables(
                        new ReflectionProviderVariableQuery()
                            .WithVisibleFrom(nodeGraph.Class.Type)
                            .WithType(baseType)
                            .WithStatic(false)));

                        AddSuggestionsWithCategory("This Methods", App.ReflectionProvider.GetMethods(
                            new ReflectionProviderMethodQuery()
                                .WithType(baseType)
                                .WithVisibleFrom(nodeGraph.Class.Type)
                                .WithStatic(false)));
                    }

                    AddSuggestionsWithCategory("Static Methods", App.ReflectionProvider.GetMethods(
                        new ReflectionProviderMethodQuery()
                            .WithStatic(true)
                            .WithVisibleFrom(nodeGraph.Class.Type)));

                    AddSuggestionsWithCategory("Static Variables", App.ReflectionProvider.GetVariables(
                        new ReflectionProviderVariableQuery()
                            .WithStatic(true)
                            .WithVisibleFrom(nodeGraph.Class.Type)));
                }
                else if (nodeGraph is ClassGraph)
                {
                    AddSuggestionsWithCategory("Types", App.ReflectionProvider.GetNonStaticTypes());
                }
            }

            return suggestions.Distinct();
        }
    }
}
