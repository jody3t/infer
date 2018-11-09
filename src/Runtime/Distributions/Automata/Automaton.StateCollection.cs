﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.ML.Probabilistic.Distributions.Automata
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Microsoft.ML.Probabilistic.Core.Collections;
    using Microsoft.ML.Probabilistic.Distributions;
    using Microsoft.ML.Probabilistic.Math;
    using Microsoft.ML.Probabilistic.Utilities;

    public abstract partial class Automaton<TSequence, TElement, TElementDistribution, TSequenceManipulator, TThis>
        where TSequence : class, IEnumerable<TElement>
        where TElementDistribution : IDistribution<TElement>, SettableToProduct<TElementDistribution>, SettableToWeightedSumExact<TElementDistribution>, CanGetLogAverageOf<TElementDistribution>, SettableToPartialUniform<TElementDistribution>, new()
        where TSequenceManipulator : ISequenceManipulator<TSequence, TElement>, new()
        where TThis : Automaton<TSequence, TElement, TElementDistribution, TSequenceManipulator, TThis>, new()
    {
        /// <summary>
        /// Represents a collection of automaton states for use in public APIs
        /// </summary>
        /// <remarks>
        /// Is a thin wrapper around Automaton.stateData. Wraps each <see cref="StateData"/> into <see cref="State"/> on demand.
        /// </remarks>
        public struct StateCollection : IReadOnlyList<State>
        {
            /// <summary>
            /// TODO
            /// </summary>
            internal ReadOnlyArray<StateData> states;

            internal ReadOnlyArray<Transition> transitions;

            /// <summary>
            /// Owner automaton of all states in collection.
            /// </summary>
            private readonly Automaton<TSequence, TElement, TElementDistribution, TSequenceManipulator, TThis> owner;

            /// <summary>
            /// Initializes instance of <see cref="StateCollection"/>.
            /// </summary>
            internal StateCollection(
                Automaton<TSequence, TElement, TElementDistribution, TSequenceManipulator, TThis> owner,
                ReadOnlyArray<StateData> states,
                ReadOnlyArray<Transition> transitions)
            {
                this.owner = owner;
                this.states = states;
                this.transitions = transitions;
            }

/*
            /// <summary>
            /// Removes the state with a given index from the automaton.
            /// </summary>
            /// <param name="index">The index of the state to remove.</param>
            /// <remarks>
            /// The automaton representation we currently use does not allow for fast state removal.
            /// Ideally we should get rid of this function completely.
            /// </remarks>
            internal void Remove(int index)
            {
                Debug.Assert(index >= 0 && index < this.states.Count, "An invalid state index provided.");
                Debug.Assert(index != this.owner.startStateIndex, "Cannot remove the start state.");

                this.states.RemoveAt(index);
                var stateCount = this.states.Count;
                for (var i = 0; i < stateCount; i++)
                {
                    StateData state = this.states[i];
                    for (var j = state.TransitionCount - 1; j >= 0; j--)
                    {
                        var transition = state.GetTransition(j);
                        if (transition.DestinationStateIndex == index)
                        {
                            state.RemoveTransition(j);
                        }

                        if (transition.DestinationStateIndex > index)
                        {
                            transition.DestinationStateIndex = transition.DestinationStateIndex - 1;
                        }

                        state.SetTransition(j, transition);
                    }
                }
            }
*/

            #region IReadOnlyList<State> methods

            /// <summary>
            /// Gets state by its index.
            /// </summary>
            public State this[int index] => new State(this.owner, this.states, this.transitions, index);

            /// <summary>
            /// Gets number of states in collection.
            /// </summary>
            public int Count => this.states.Count;

            /// <summary>
            /// Returns enumerator over all states in collection.
            /// </summary>
            public IEnumerator<State> GetEnumerator()
            {
                // TODO: optimize! (introduce ValueType enumerator)
                var owner = this.owner;
                var states = this.states;
                var transitions = this.transitions;
                return this.states.Select((data, index) => new State(owner, states, transitions, index)).GetEnumerator();
            }

            /// <summary>
            /// Returns enumerator over all states in collection.
            /// </summary>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            #endregion

            public void SetTo(StateCollection that)
            {
                this.states = that.states;
                this.transitions = that.transitions;
            }

            public void SwapWith(ref StateCollection that)
            {
                Util.Swap(ref this.states, ref that.states);
                Util.Swap(ref this.transitions, ref that.transitions);
            }
        }
    }
}