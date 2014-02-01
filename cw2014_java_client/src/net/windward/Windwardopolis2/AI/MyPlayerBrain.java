/*
 * ----------------------------------------------------------------------------
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with this
 * stuff. If you meet an employee from Windward some day, and you think this
 * stuff is worth it, you can buy them a beer in return. Windward Studios
 * ----------------------------------------------------------------------------
 */

package net.windward.Windwardopolis2.AI;

import net.windward.Windwardopolis2.api.*;
import org.apache.log4j.Logger;

import java.awt.*;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.URLDecoder;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;

/**
 * The sample C# AI. Start with this project but write your own code as this is a very simplistic implementation of the AI.
 */
public class MyPlayerBrain implements net.windward.Windwardopolis2.AI.IPlayerAI {
    private static String NAME = "Стая лошадей";

    // bugbug - put your school name here. Must be 11 letters or less (ie use MIT, not Massachussets Institute of Technology).
    public static String SCHOOL = "Purdue U.";

    private static Logger log = Logger.getLogger(IPlayerAI.class);

    /**
     * The name of the player.
     */
    private String privateName;

    public final String getName() {
        return privateName;
    }

    private void setName(String value) {
        privateName = value;
    }

    /**
     * The game map.
     */
    private Map privateGameMap;

    public final Map getGameMap() {
        return privateGameMap;
    }

    private void setGameMap(Map value) {
        privateGameMap = value;
    }

    /**
     * All of the players, including myself.
     */
    private java.util.ArrayList<Player> privatePlayers;

    public final java.util.ArrayList<Player> getPlayers() {
        return privatePlayers;
    }

    private void setPlayers(java.util.ArrayList<Player> value) {
        privatePlayers = value;
    }

    /**
     * All of the companies.
     */
    private java.util.ArrayList<Company> privateCompanies;

    public final java.util.ArrayList<Company> getCompanies() {
        return privateCompanies;
    }

    private void setCompanies(java.util.ArrayList<Company> value) {
        privateCompanies = value;
    }

    /**
     * All of the passengers.
     */
    private java.util.ArrayList<Passenger> privatePassengers;

    public final java.util.ArrayList<Passenger> getPassengers() {
        return privatePassengers;
    }

    private void setPassengers(java.util.ArrayList<Passenger> value) {
        privatePassengers = value;
    }

    /**
     * All of the coffee stores.
     */
    private java.util.ArrayList<CoffeeStore> privateStores;

    public final ArrayList<CoffeeStore> getCoffeeStores() { return privateStores; }

    private void setCoffeeStores(ArrayList<CoffeeStore> value) { privateStores = value; }

    /**
     * The power up deck
     */
    private ArrayList<PowerUp> privatePowerUpDeck;

    public final ArrayList<PowerUp> getPowerUpDeck() { return privatePowerUpDeck; }

    private void setPowerUpDeck(ArrayList<PowerUp> value) { privatePowerUpDeck = value; }


    /**
     * My power up hand
     */
    private ArrayList<PowerUp> privatePowerUpHand;

    public final ArrayList<PowerUp> getPowerUpHand() { return privatePowerUpHand; }

    private void setPowerUpHand(ArrayList<PowerUp> value) { privatePowerUpHand = value; }

    /**
     * Me (my player object).
     */
    private Player privateMe;

    public final Player getMe() {
        return privateMe;
    }

    private void setMe(Player value) {
        privateMe = value;
    }

    /**
     * My current passenger
     */
    private Passenger privateMyPassenger;

    public final Passenger getMyPassenger() { return privateMyPassenger; }

    private void setMyPassenger(Passenger value) { privateMyPassenger = value; }


    private PlayerAIBase.PlayerOrdersEvent sendOrders;

    private PlayerAIBase.PlayerCardEvent playCards;

    /**
     * The maximum number of trips allowed before a refill is required.
     */
    private static final int MAX_TRIPS_BEFORE_REFILL = 3;

    private static final java.util.Random rand = new java.util.Random();

    public MyPlayerBrain(String name) {
        setName(!net.windward.Windwardopolis2.DotNetToJavaStringHelper.isNullOrEmpty(name) ? name : NAME);
        privatePowerUpHand = new ArrayList<PowerUp>();
    }

    /**
     * The avatar of the player. Must be 32 x 32.
     */
    public final byte[] getAvatar() {
        try {
            // open image
            InputStream stream = getClass().getResourceAsStream("/net/windward/Windwardopolis2/res/MyAvatar.png");

            byte [] avatar = new byte[stream.available()];
            stream.read(avatar, 0, avatar.length);
            return avatar;

        } catch (IOException e) {
            System.out.println("error reading image");
            e.printStackTrace();
            return null;
        }
    }


    /**
     * Called at the start of the game.
     *
     * @param map         The game map.
     * @param me          You. This is also in the players list.
     * @param players     All players (including you).
     * @param companies   The companies on the map.
     * @param passengers  The passengers that need a lift.
     * @param ordersEvent Method to call to send orders to the server.
     */
    public final void Setup(Map map, Player me, java.util.ArrayList<Player> players, java.util.ArrayList<Company> companies, ArrayList<CoffeeStore> stores,
                            java.util.ArrayList<Passenger> passengers, ArrayList<PowerUp> powerUps, PlayerAIBase.PlayerOrdersEvent ordersEvent, PlayerAIBase.PlayerCardEvent cardEvent) {

        try {
            setGameMap(map);
            setPlayers(players);
            setMe(me);
            setCompanies(companies);
            setPassengers(passengers);
            setCoffeeStores(stores);
            setPowerUpDeck(powerUps);
            sendOrders = ordersEvent;
            playCards = cardEvent;

            java.util.ArrayList<Passenger> pickup = AllPickups(this, me, passengers);

            // get the path from where we are to the dest.
            java.util.ArrayList<Point> path = CalculatePathPlus1(me, pickup.get(0).getLobby().getBusStop());
            sendOrders.invoke("ready", path, pickup);
        } catch (RuntimeException ex) {
            log.fatal("setup(" + me == null ? "NULL" : me.getName() + ") Exception: " + ex.getMessage());
            ex.printStackTrace();

        }
    }

    /**
     * Called to send an update message to this A.I. We do NOT have to send orders in response.
     *
     * @param status     The status message.
     * @param plyrStatus The player this status is about. THIS MAY NOT BE YOU.
     */
    public final void GameStatus(PlayerAIBase.STATUS status, Player plyrStatus)
    {

        try
        {
            Point ptDest = null;
            java.util.ArrayList<Passenger> pickup = new java.util.ArrayList<Passenger>();

            if (plyrStatus != getMe())
            {
                //If there is an enemy at our destination, abandon this passenger
                if (getMyPassenger() != null && checkForEnemy())
                {
                    pickup = AllPickups(this, getMe(), getPassengers());
                    ptDest = pickup.get(0).getLobby().getBusStop();
                    System.out.println("Enemy detected at " + getMyPassenger().getDestination() + ".  Rerouting.");
                    DisplayOrders(ptDest);

                    // get the path from where we are to the dest.
                    java.util.ArrayList<Point> path = CalculatePathPlus1(getMe(), ptDest);

                    // update our saved Player to match new settings
                    if (path.size() > 0) {
                        getMe().getLimo().getPath().clear();
                        getMe().getLimo().getPath().addAll(path);
                    }
                    if (pickup.size() > 0) {
                        getMe().getPickUp().clear();
                        getMe().getPickUp().addAll(pickup);
                    }

                    sendOrders.invoke("move", path, pickup);
                }

                //Check once again for closest passenger
                if (getMyPassenger() == null)
                {
                    //If we're out of coffee, go get coffee from the nearest store
                    if (getMe().getLimo().getCoffeeServings() < 1)
                    {
                        ptDest = GetCoffeeQueue(this, getCoffeeStores()).get(0).getBusStop();
                    }
                    else
                    {
                        pickup = AllPickups(this, getMe(), getPassengers());
                        ptDest = pickup.get(0).getLobby().getBusStop();
                    }
                }
                return;
            }

            if(status == PlayerAIBase.STATUS.UPDATE) {
                MaybePlayPowerUp();
                return;
            }

            DisplayStatus(status, plyrStatus);

            if(log.isDebugEnabled())
                log.info("gameStatus( " + status + " )");

            switch (status) {
                case NO_PATH:
                case PASSENGER_NO_ACTION:
                    if (getMe().getLimo().getPassenger() == null) {
                        pickup = AllPickups(this, plyrStatus, getPassengers());
                        ptDest = pickup.get(0).getLobby().getBusStop();
                    } else {
                        ptDest = getMe().getLimo().getPassenger().getDestination().getBusStop();
                    }
                    break;
                case PASSENGER_DELIVERED:
                case PASSENGER_ABANDONED:
                    pickup = AllPickups(this, getMe(), getPassengers());
                    ptDest = pickup.get(0).getLobby().getBusStop();
                    break;
                case PASSENGER_REFUSED_ENEMY:
                    //Get a new list of pickups and pretend we don't have a passenger
					pickup = AllPickups(this, getMe(), getPassengers());
                    ptDest = pickup.get(0).getLobby().getBusStop();
					
					/* Their code
                    java.util.List<Company> comps = getCompanies();
                    while(ptDest == null) {
                        int randCompany = rand.nextInt(comps.size());
                        if (comps.get(randCompany) != getMe().getLimo().getPassenger().getDestination()) {
                            ptDest = comps.get(randCompany).getBusStop();
                            break;
                        }
                    }
					*/
                    break;
                case PASSENGER_DELIVERED_AND_PICKED_UP:
                case PASSENGER_PICKED_UP:
                    pickup = AllPickups(this, getMe(), getPassengers());
                    ptDest = getMe().getLimo().getPassenger().getDestination().getBusStop();
                    break;

            }

            // coffee store override
            switch (status)
            {
                case PASSENGER_DELIVERED_AND_PICKED_UP:
                case PASSENGER_DELIVERED:
                case PASSENGER_ABANDONED:
                case PASSENGER_REFUSED_NO_COFFEE:
                case PASSENGER_DELIVERED_AND_PICK_UP_REFUSED:
                    //If we're out of coffee and don't have a passenger, go get coffee from the nearest store
                    if (getMe().getLimo().getCoffeeServings() < 1 && getMyPassenger() == null)
                    {
                        ptDest = GetCoffeeQueue(this, getCoffeeStores()).get(0).getBusStop();
                        System.out.println("Setting location to a coffee place");
                    }
                    break;
                case COFFEE_STORE_CAR_RESTOCKED:
                    pickup = AllPickups(this, getMe(), getPassengers());
                    if (pickup.size() == 0)
                        break;
                    ptDest = pickup.get(0).getLobby().getBusStop();
                    break;
            }

            //If we're out of coffee and don't have a passenger, go get coffee from the nearest store
            if (getMe().getLimo().getCoffeeServings() < 1)
            {
                ptDest = GetCoffeeQueue(this, getCoffeeStores()).get(0).getBusStop();
                System.out.println("Setting location to a coffee place");
            }

            // may be another status
            if(ptDest == null)
                return;

            // get the path from where we are to the dest.
            java.util.ArrayList<Point> path = CalculatePathPlus1(getMe(), ptDest);

            //If we have no passenger and a coffee store is on the way to the next passenger (or close), go get coffee
            if (getMyPassenger() == null && getMe().getLimo().getCoffeeServings() != 3)
            {
                boolean found = false;
                for (CoffeeStore cs : GetCoffeeQueue(this, getCoffeeStores()))
                {
                    Point loc = cs.getBusStop();
                    for (Point p : path)
                    {
                        //If a coffee shop is within 4 units of our desired path, go there first
                        if (getDistTo(loc, p) < 4)
                        {
                            ptDest = cs.getBusStop();
                            path = CalculatePathPlus1(getMe(), ptDest);
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        break;
                }
            }

            DisplayOrders(ptDest);

            if (log.isDebugEnabled())
            {
                log.debug(status + "; Path:" + (path.size() > 0 ? path.get(0).toString() : "{n/a}") + "-" + (path.size() > 0 ? path.get(path.size()-1).toString() : "{n/a}") + ", " + path.size() + " steps; Pickup:" + (pickup.size() == 0 ? "{none}" : pickup.get(0).getName()) + ", " + pickup.size() + " total");
            }

            // update our saved Player to match new settings
            if (path.size() > 0) {
                getMe().getLimo().getPath().clear();
                getMe().getLimo().getPath().addAll(path);
            }
            if (pickup.size() > 0) {
                getMe().getPickUp().clear();
                getMe().getPickUp().addAll(pickup);
            }

            sendOrders.invoke("move", path, pickup);
        } catch (RuntimeException ex) {
            ex.printStackTrace();
        }
    }

    private void MaybePlayPowerUp()
    {
        // draw a card
        if (getPowerUpHand().size() < getMe().getMaxCardsInHand() && getPowerUpDeck().size() > 0)
        {
            for (int index = 0; index < getMe().getMaxCardsInHand() - getPowerUpHand().size() && getPowerUpDeck().size() > 0; index++)
            {
                // select a card
                PowerUp pu = getPowerUpDeck().get(0);
                privatePowerUpDeck.remove(pu);
                privatePowerUpHand.add(pu);
                playCards.invoke(PlayerAIBase.CARD_ACTION.DRAW, pu);
            }
        }

        int numCoffees = getMe().getLimo().getCoffeeServings();
        if (getPowerUpHand().size() != 0)
        {
            //Can we play?
            PowerUp pu2 = null;
            for(PowerUp current : getPowerUpHand()) {
                if(current.isOkToPlay()) {
                    pu2 = current;
                    break;
                }
            }
            if (pu2 == null)
                return;

            //Discard this crap
            if (pu2.getCard() == PowerUp.CARD.MULT_DELIVERY_QUARTER_SPEED)
            {
                playCards.invoke(PlayerAIBase.CARD_ACTION.DISCARD, pu2);
                privatePowerUpHand.remove(pu2);
            }
            else if (numCoffees > 1 && pu2.getCard() == PowerUp.CARD.MOVE_PASSENGER)
            {
                Passenger toUseCardOn = null;
                for(Passenger pass : privatePassengers)
                {
                    if(pass.getCar() == null)
                    {
                        toUseCardOn = pass;
                        break;
                    }
                }
                pu2.setPassenger(toUseCardOn);
            }
            else if (pu2.getCard() == PowerUp.CARD.CHANGE_DESTINATION || pu2.getCard() == PowerUp.CARD.STOP_CAR)
            {
                java.util.ArrayList<Player> plyrsWithPsngrs = new ArrayList<Player>();
                for(Player play : privatePlayers) {
                    if(play.getGuid() != getMe().getGuid() && play.getLimo().getPassenger() != null) {
                        plyrsWithPsngrs.add(play);
                    }
                }

                if (plyrsWithPsngrs.size() == 0)
                    return;
                Player target = plyrsWithPsngrs.get(0);
                for (Player p : plyrsWithPsngrs)
                    if (p.getScore() > target.getScore())
                        target = p;
                pu2.setPlayer(target);
            }

            playCards.invoke(PlayerAIBase.CARD_ACTION.PLAY, pu2);
            System.out.println("NOTE: Playing card " + pu2.getName());
            privatePowerUpHand.remove(pu2);
        }
    }

    /**
     * A power-up was played. It may be an error message, or success.
     * @param puStatus - The status of the played card.
     * @param plyrPowerUp - The player who played the card.
     * @param cardPlayed - The card played.
     */
    public void PowerupStatus(PlayerAIBase.STATUS puStatus, Player plyrPowerUp, PowerUp cardPlayed)
    {
        // redo the path if we got relocated
        if ((puStatus == PlayerAIBase.STATUS.POWER_UP_PLAYED) && ((cardPlayed.getCard() == PowerUp.CARD.RELOCATE_ALL_CARS) ||
                ((cardPlayed.getCard() == PowerUp.CARD.CHANGE_DESTINATION) && (cardPlayed.getPlayer() != null ? cardPlayed.getPlayer().getGuid() : null) == getMe().getGuid())))
            GameStatus(PlayerAIBase.STATUS.NO_PATH, getMe());
    }

    private void DisplayStatus(PlayerAIBase.STATUS status, Player plyrStatus)
    {
        String msg = null;
        switch (status)
        {
            case PASSENGER_DELIVERED:
                msg = getMyPassenger().getName() + " delivered to " + getMyPassenger().getLobby().getName();
                privateMyPassenger = null;
                break;
            case PASSENGER_ABANDONED:
                msg = getMyPassenger().getName() + " abandoned at " + getMyPassenger().getLobby().getName();
                privateMyPassenger = null;
                break;
            case PASSENGER_REFUSED_ENEMY:
                msg = plyrStatus.getLimo().getPassenger().getName() + " refused to exit at " +
                        plyrStatus.getLimo().getPassenger().getDestination().getName() + " - enemy there";
                break;
            case PASSENGER_DELIVERED_AND_PICKED_UP:
                msg = getMyPassenger().getName() + " delivered at " + getMyPassenger().getLobby().getName() + " and " +
                        plyrStatus.getLimo().getPassenger().getName() + " picked up (Destination: " + plyrStatus.getLimo().getPassenger().getDestination() + ")";
                privateMyPassenger = plyrStatus.getLimo().getPassenger();
                break;
            case PASSENGER_PICKED_UP:
                msg = plyrStatus.getLimo().getPassenger().getName() + " picked up (Destination: " + plyrStatus.getLimo().getPassenger().getDestination() + ")";
                privateMyPassenger = plyrStatus.getLimo().getPassenger();
                break;
            case PASSENGER_REFUSED_NO_COFFEE:
                msg = "Passenger refused to board limo, no coffee";
                break;
            case PASSENGER_DELIVERED_AND_PICK_UP_REFUSED:
                msg = getMyPassenger().getName() + " delivered at " + getMyPassenger().getLobby().getName() +
                        ", new passenger refused to board limo, no coffee";
                break;
            case COFFEE_STORE_CAR_RESTOCKED:
                msg = "Coffee restocked!";
                break;
        }
        if (msg != null && !msg.equals(""))
        {
            System.out.println(msg);
            if (log.isInfoEnabled())
                log.info(msg);
        }
    }

    private void DisplayOrders(Point ptDest)
    {
        String msg = null;
        CoffeeStore store = null;
        for(CoffeeStore s : getCoffeeStores()) {
            if(s.getBusStop() == ptDest) {
                store = s;
                break;
            }
        }

        if (store != null)
            msg = "Heading toward " + store.getName() + " at " + ptDest.toString();
        else
        {
            Company company = null;
            for(Company c : getCompanies()) {
                if(c.getBusStop() == ptDest) {
                    company = c;
                    break;
                }
            }

            if (company != null)
                msg = "Heading toward " + company.getName() + " at " + ptDest.toString();
        }
        if (msg != null && !msg.equals(""))
        {
            System.out.println(msg);
            if (log.isInfoEnabled())
                log.info(msg);
        }
    }

    private java.util.ArrayList<Point> CalculatePathPlus1(Player me, Point ptDest) {
        java.util.ArrayList<Point> path = SimpleAStar.CalculatePath(getGameMap(), me.getLimo().getMapPosition(), ptDest);
        // add in leaving the bus stop so it has orders while we get the message saying it got there and are deciding what to do next.
        if (path.size() > 1) {
            path.add(path.get(path.size() - 2));
        }
        return path;
    }

    private int getDistTo(Point p1, Point p2)
    {
        return SimpleAStar.CalculatePath(getGameMap(), p1, p2).size();
    }

    private boolean checkForEnemy()
    {
        for (Passenger p : getMyPassenger().getDestination().getPassengers())
            if (getMyPassenger().getEnemies().contains(p))
            {
                System.out.println("***ENEMY DETECTED***");
                return true;
            }
        return false;
    }

    private static ArrayList<CoffeeStore> GetCoffeeQueue(final MyPlayerBrain ai, ArrayList<CoffeeStore> stores)
    {
        Collections.sort(stores, new Comparator<CoffeeStore>()
        {
           public int compare(CoffeeStore c1, CoffeeStore c2)
           {
               return ai.getDistTo(ai.getMe().getLimo().getMapPosition(), c1.getBusStop()) - ai.getDistTo(ai.getMe().getLimo().getMapPosition(), c2.getBusStop());
           }
        });
        return stores;
    }

    private static java.util.ArrayList<Passenger> AllPickups(final MyPlayerBrain ai, Player me, Iterable<Passenger> passengers) {
        java.util.ArrayList<Passenger> pickup = new java.util.ArrayList<Passenger>();

        for (Passenger psngr : passengers) {
            if ((!me.getPassengersDelivered().contains(psngr)) && (psngr != me.getLimo().getPassenger()) && (psngr.getCar() == null) && (psngr.getLobby() != null) && (psngr.getDestination() != null))
                pickup.add(psngr);
        }

        //add sort by random so no loops for can't pickup
		Collections.sort(pickup, new Comparator<Passenger>()
		{
			public int compare(Passenger p1, Passenger p2)
			{
				boolean p1HasEnemy = false;
				boolean p2HasEnemy = false;
				
				//If the passenger has an enemy at their destination, do not take them
				for (Passenger enemy : p1.getEnemies())
                {
                    Company eLoc = enemy.getLobby();
					if (eLoc != null && eLoc.getBusStop().equals(p1.getDestination().getBusStop()))
					{
						p1HasEnemy = true;
						break;
					}
                }
				
				for (Passenger enemy : p2.getEnemies())
                {
                    Company eLoc = enemy.getLobby();
					if (eLoc != null && eLoc.getBusStop().equals(p2.getDestination().getBusStop()))
					{
						p2HasEnemy = true;
						break;
					}
                }
				
				if (p1HasEnemy && !p2HasEnemy)
					return 1;
				else if (p2HasEnemy && !p1HasEnemy)
					return -1;
				
				//If the passenger is right here, go ahead and take them
				int dist2p1 = ai.getDistTo(ai.getMe().getLimo().getMapPosition(), p1.getLobby().getBusStop());
				int dist2p2 = ai.getDistTo(ai.getMe().getLimo().getMapPosition(), p2.getLobby().getBusStop());
				if (dist2p1 == 1 && dist2p2 != 1)
					return -1;
				else if (dist2p2 == 1)
					return 1;
				//Otherwise, just find the shortest total length required
				else
				{
					int dist1 = dist2p1 * dist2p1 + ai.getDistTo(p1.getLobby().getBusStop(), p1.getDestination().getBusStop());
					int dist2 = dist2p2 * dist2p2 + ai.getDistTo(p2.getLobby().getBusStop(), p2.getDestination().getBusStop());
					return dist1 - dist2;
				}
			}
		});
        return pickup;
    }
}