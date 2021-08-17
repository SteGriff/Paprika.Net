using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paprika.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paprika.NetTests
{
    [TestClass()]
    public class IntegrationTests
    {
        [TestMethod()]
        public void IntegrationTest1()
        {
            var engine = new PaprikaEngine();
            var grammar = @"
* time to buy tweet
[today i will/time to/planning to] [buy] [the worlds] [most powerful] [thing to buy]
[today i will/time to/planning to/please help me] [buy] the [most powerful] [thing to buy] [in the world]

* buy
buy
acquire
find
locate
commandeer

* the worlds
the world's
the planet's
the earth's
the universe's
the nation's
the galaxy's

* in the world
in the world
on this [good/green/great/grand] earth
on the planet
in the [universe/nation/galaxy]

* most powerful
most powerful
most flammable
loudest
mightiest

* thing to buy
ac adaptor
cpu fan
water feature
[petrol/diesel] generator
";
            var lines = grammar.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            engine.LoadGrammarFromString(lines);

            string input = "[time to buy tweet]";
            var wantedOutcomes = new[] {
                "today i will buy the world's most powerful cpu fan",
                "time to acquire the most flammable petrol generator on this green earth",
                "please help me find the loudest water feature on the planet"
            };
            var foundOutcomes = new HashSet<string>();
            var foundAll = false;
            var i = 0;

            while (!foundAll)
            {
                i++;
                var actual = engine.Parse(input);
                if (wantedOutcomes.Contains(actual))
                {
                    foundOutcomes.Add(actual);
                    foundAll = foundOutcomes.Count == wantedOutcomes.Length;
                    if (foundAll) break;
                }
                else if (i > 1000000)
                {
                    Assert.Fail("Should happen more than once in a million");
                }
            }

            Assert.IsTrue(foundAll, "FoundAll");
        }

        [TestMethod()]
        public void IntegrationTest2()
        {
            var engine = new PaprikaEngine();
            var grammar = @"
* anti corporate tweet
i am [extremely] [anti corporate] [for example] [i have never used x]
i [extremely] [reject/eschew/deny] [all corporations/everything corporate] [for example] [i have never used x]

* i have never used x
[i have/i've] never [/even /once ][consumed] [[consumed] thing] [?that's right]

* extremely
passionately
radically
zealously
feverishly

* anti corporate
anti corporate
[anti/against] corporation[/s]

* for example
for example
in fact

* that's right
[,/.] that's [right/the truth]

# ----- Smart Lookup -----

* consumed
eaten
had
bought
used

* eaten thing
food
anything
greggs
beef

* had thing
[/a ]coffee i didn't [/personally ][/source and ]grind[/ myself/ with my own hands]

* bought thing
anything
[used thing]

* used thing
anything [steve jobs/bill gates] has touched
[shampoo or conditioner/real soap/soap products of any kind]
[any goods/anything] i can't [get/harvest/lovingly twease] from a plant
";
            var lines = grammar.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            engine.LoadGrammarFromString(lines);

            string input = "[anti corporate tweet]";
            var wantedOutcomes = new[] {
            "i am feverishly anti corporations for example i have never once used anything bill gates has touched",
            "i passionately eschew everything corporate in fact i've never even eaten beef . that's the truth"
        };
            var foundOutcomes = new HashSet<string>();
            var foundAll = false;
            var i = 0;

            while (!foundAll)
            {
                i++;
                var actual = engine.Parse(input);
                if (wantedOutcomes.Contains(actual))
                {
                    foundOutcomes.Add(actual);
                    foundAll = foundOutcomes.Count == wantedOutcomes.Length;
                    if (foundAll) break;
                }
                else if (i > 1000000)
                {
                    Assert.Fail("Should happen more than once in a million");
                }
            }

            Assert.IsTrue(foundAll, "FoundAll");
        }
    }
}