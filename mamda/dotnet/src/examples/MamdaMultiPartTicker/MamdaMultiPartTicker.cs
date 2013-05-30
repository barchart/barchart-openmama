/* $Id$
 *
 * OpenMAMA: The open middleware agnostic messaging API
 * Copyright (C) 2011 NYSE Technologies, Inc.
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
 * 02110-1301 USA
 */

using System;
using System.Threading;
using Wombat;
using System.Collections;

namespace Wombat.Mamda.Examples
{
	/// <summary>
	/// Example program illustrating the use of MAMDA with multi participant group
	/// subscriptions.
	/// 
	/// In this example, we create one ComboTicker for *each* of the
	/// individual participants and consolidated member of the
	/// MamdaMultiParticipantManager (i.e., one each for the BBO and each
	/// participant). (The same instance could be used for all. However, a separate
	/// MamdaMsglistener instance must be used for each.)
	/// 
	/// If the developer only wanted a subset of these members, then those
	/// could be selected in his/her implementation of
	/// MamdaMultiParticipantManager.  Similarly, the developer may not
	/// want to create listeners for both trades and quotes for each
	/// participant.
	/// </summary>
	/// <remarks>
	/// Note: if trades are reported, they will be reported twice: once
	/// for the participant and once for the consolidated.  Depending upon
	/// exchange and oversight rules, there may be differences between
	/// the trade reports for a participant versus the same report for
	/// the consolidated.  For example, by certain rules, a trade may
	/// qualify to update the official last price for an exchange but not
	/// the consolidated last price.  The aggregate volume traded will, of
	/// course, be different for each participant and the consolidated.
	/// </remarks>
	public class MamdaMultiPartTicker
	{
        private static ArrayList mamdaSubscriptions = new ArrayList();
		public static void Main(string[] args)
		{
			MamaTransport        transport    = null;
			MamaQueue            defaultQueue = null;
			MamaDictionary       dictionary   = null;
			CommandLineProcessor options      = new CommandLineProcessor(args);
			double               throttleRate = options.getThrottleRate();

            
			if (options.hasLogLevel())
			{
				logLevel_ = options.getLogLevel();
				Mama.enableLogging(logLevel_);
			}

			if ((throttleRate > 100.0) || (throttleRate <= 0.0))
			{
				throttleRate = 100.0;
			}

			try
			{
				// Initialize MAMDA
				myBridge = new MamaBridge(options.getMiddleware());
				Mama.open();

				transport = new MamaTransport();
				transport.create(options.getTransport(), myBridge);
				defaultQueue = Mama.getDefaultEventQueue(myBridge);
				//Get the Data dictionary.....
				MamaSource dictionarySource = new MamaSource();
                dictionarySource.symbolNamespace = options.getDictSource();
				dictionarySource.transport = transport;
				dictionary = buildDataDictionary(transport, defaultQueue, dictionarySource);

				//Initialise the dictionary and fields
				MamdaCommonFields.setDictionary(dictionary, null);
				MamdaQuoteFields.setDictionary(dictionary, null);
				MamdaTradeFields.setDictionary(dictionary, null);

				foreach (string symbol in options.getSymbolList())
				{
					MamdaSubscription aSubscription = new MamdaSubscription();
					aSubscription.setType(mamaSubscriptionType.MAMA_SUBSC_TYPE_GROUP);
					aSubscription.create(
						transport,
                        defaultQueue,
						options.getSource(),
						symbol,
						null);

					/* For each subscription a MamdaMultiParticipantManager is
					* added as the message listener. The callbacks on the
					* MamdaMultiPartHandler will be invokes as new group members
					* become available.*/
					MamdaMultiParticipantManager multiPartManager =
						new MamdaMultiParticipantManager(symbol);
					multiPartManager.addHandler(new MultiPartHandler());

					aSubscription.addMsgListener(multiPartManager);

					aSubscription.activate();

                   mamdaSubscriptions.Add(aSubscription);
				}

				Mama.start(myBridge);
				GC.KeepAlive(dictionary);
				Console.WriteLine("Press ENTER or Ctrl-C to quit...");
				Console.ReadLine();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Environment.Exit(1);
			}
		}

		/// <summary>
		/// Implementation of the <code>MamdaMultiParticipantHandler</code>
		/// interface.
		/// 
		/// Here we are adding a trade and quote listener for every participant and
		/// consolidated update as part of the underlying group subscription. The
		/// assumption here is that we are interested in all trade and quote
		/// information for all participants.
		/// </summary>
		private class MultiPartHandler : MamdaMultiParticipantHandler
		{
			public void onConsolidatedCreate(
				MamdaSubscription              subscription,
				MamdaMultiParticipantManager   manager)
			{
				MamdaTradeListener aTradeListener = new MamdaTradeListener();
				MamdaQuoteListener aQuoteListener = new MamdaQuoteListener();
				ComboTicker        aTicker        = new ComboTicker();

				aTradeListener.addHandler(aTicker);
				aQuoteListener.addHandler(aTicker);

				manager.addConsolidatedListener(aTradeListener);
				manager.addConsolidatedListener(aQuoteListener);
			}

			public void onParticipantCreate (
				MamdaSubscription             subscription,
				MamdaMultiParticipantManager  manager,
				string                        participantId,
				NullableBool                   isPrimary)
			{
				MamdaTradeListener aTradeListener = new MamdaTradeListener();
				MamdaQuoteListener aQuoteListener = new MamdaQuoteListener();
				ComboTicker        aTicker        = new ComboTicker();

				aTradeListener.addHandler(aTicker);
				aQuoteListener.addHandler(aTicker);

				manager.addParticipantListener(aTradeListener, participantId);
				manager.addParticipantListener(aQuoteListener, participantId);
			}
		}

		private static MamaDictionary buildDataDictionary(
			MamaTransport transport,
			MamaQueue defaultQueue,
			MamaSource dictionarySource)
		{
			bool[] gotDict = new bool[] { false };
			MamaDictionaryCallback dictionaryCallback = new DictionaryCallback(gotDict);
			lock (dictionaryCallback)
			{
				MamaSubscription subscription = new MamaSubscription ();

				MamaDictionary dictionary = new MamaDictionary();
				dictionary.create(
					defaultQueue,
					dictionaryCallback,
					dictionarySource,
					3,
					10);

				Mama.start(myBridge);
				if (!gotDict[0])
				{
					if (!Monitor.TryEnter(dictionaryCallback, 30000))
					{
						Console.Error.WriteLine("Timed out waiting for dictionary.");
						Environment.Exit(0);
					}
					Monitor.Exit(dictionaryCallback);
				}
				return dictionary;
			}
		}

		private class DictionaryCallback : MamaDictionaryCallback
		{
			public DictionaryCallback(bool[] gotDict)
			{
				gotDict_ = gotDict;
			}

			public void onTimeout(MamaDictionary dictionary)
			{
				Console.Error.WriteLine("Timed out waiting for dictionary");
				Environment.Exit(1);
			}

			public void onError(MamaDictionary dictionary, string message)
			{
				Console.Error.WriteLine("Error getting dictionary: {0}", message);
				Environment.Exit(1);
			}

			public void onComplete(MamaDictionary dictionary)
			{
				lock (this)
				{
					gotDict_[0] = true;
					Mama.stop(myBridge);
					Monitor.PulseAll(this);
				}
			}

			private bool[] gotDict_;
		}

		private static MamaLogLevel	logLevel_ = MamaLogLevel.MAMA_LOG_LEVEL_NORMAL;
		private static object       guard_ = new object();
		private static MamaBridge	myBridge = null;
	}
}
