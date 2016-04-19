Feature: ImportLogData

Scenario Outline: Import ISO to Adapt
	Given I have datacard <cardname>
	When I import with the plugin
	Then iso is imported to adapt
Examples:
| cardname                      |
| agco_c100_tc___jd_sprayer_900 |
| KV                            |

Scenario Outline: Import ISO to Adapt Export Adapt to ISO
	Given I have datacard <cardname>
	When I import with the plugin
	And I export to Iso
	Then Adapt is exported to ISO
Examples:
| cardname                      |
| agco_c100_tc___jd_sprayer_900 |
| KV                            |