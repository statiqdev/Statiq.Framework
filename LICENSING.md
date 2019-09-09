# Statiq Licensing FAQs

## How is Statiq licensed?

Statiq is dual-licensed under the [License Zero Prosperity Public License](https://licensezero.com/licenses/prosperity) and the [License Zero Private License](https://licensezero.com/licenses/private). The Prosperity License limits commercial use to a 32 day trial period, after which a license fee must be paid to obtain a Private License.

A [License Zero Waiver](https://licensezero.com/licenses/waiver) may also be granted waiving the non-commercial clause of the Prosperity License without requiring the purchase of a Private License. These are granted on a case-by-case basis and usually when the person requesting one makes a compelling case about why the non-commercial clause shouldn't apply to them.

## How much is the license fee?

The license fee is currently $50 per developer or user, for each major version, and for no less than one calendar year. Licensing a particular major version of Statiq gives you the right to use that major version and all minor and patch versions after it, in a commercial enterprise, forever. When a new major version of Statiq is released, you must obtain a new license unless your existing license was purchased within a one year grace period, in which case you will automatically be granted a waiver to the next major version when it's released.

Note that only individuals can obtain a license (though they can obtain one within a professional context and use it for professional purposes). [Read this blog post](https://blog.licensezero.com/2018/01/26/developer-licensing.html) for more details on why only individuals are licensed.

## If only individuals can obtain a license, can I use my corporate credit card?

Yes. The licensee must be an individual but the funds for obtaining the license can come from any source.

## What if I want to use Statiq on a server?

That's fine as long as all developers that created and provisioned everything have a license.

## What if I work for a non-profit?

Then no license is needed.

## What if I work for a commercial enterprise but the use of Statiq doesn't directly impact our profit-making activities?

To the extent that everything a commercial enterprise does is typically directed towards commercial advantage in one way or another then yes, you must obtain a license.

## What if I run a blog, user group, or other open source project that places ads on our site?

It depends on whether the user is engaged in gaining commercial advantage. In general, if the blog, user group, open source, etc. site is using ads to offset costs or otherwise support non-commercial activity then no license is needed. Otherwise, if the site is intended to earn money for the purpose of profit then a license will be required.

## Do content writers and other people that work on the inputs to the tool need licenses?

No. Unless the user is running Statiq directly, using Statiq Framework in a library or application, or instructing a server how to run Statiq they do not need a license. It's expected that at least one person in each commercial entity will be performing these tasks and require a license.

## What software does the Private License cover?

The Private License granted upon paying the license fee covers all software and projects under the Statiq organization that require licensing. For example, it grants you the right to use both Statiq and the Statiq Framework for commercial purposes. Official themes and other extensions are often licensed under more permissive terms and don't typically require a license in any case.

There are no other additional rights or privileges granted under the Private License. Specifically, it does not guarantee enhanced (or even any) support or priority issue resolution.

## You mean after I pay the license fee I don't get support?

That's correct. This is still a notionally open source project and a small licensing fee does not cover an increased support burden. That said, I do my best to eventually respond to every support request (if not resolve them right away).

## If I purchase a license, do other users of the application I built with Statiq also require one?

No. In general the Private License includes provisions for sublicensing to your users as long as they don't then use the tools to create their own software.

## But this isn't actually open source!

You're correct, at least for the "official" definition of open source. Both the [Free Software Foundation](https://www.gnu.org/philosophy/free-sw.en.html) and the [Open Source Initiative](https://opensource.org/osd-annotated) explicitly exclude software that contains limits on use from the definition of "open source". For this reason, the Prosperity License this project uses is not, and likely never will be, an OSI-approved license.

All that said, I think this definition of open source is in need of some reality checks. Without getting into too much detail in this FAQ, the open source community has a sustainability problem. As projects grow in size many maintainers burn-out or find themselves unable to satisfy increasing support and maintenance demands. Finding reliable, well-defined funding sources is extremely challenging (donations simply don't work for the vast majority of projects) and while funding isn't the only or exclusive way to address open source sustainability, it's certainly one component of an overall approach. As a community if we're going to try and improve the sustainability of the projects we rely on, and the health of their maintainers, then we have to expand the umbrella of open source to allow that maybe, just maybe, compensating maintainers for the large amount of time they dedicate to creating those projects, and placing restrictions on the use of those projects as an enforcement mechanism, isn't such a bad idea.

I understand that "free software" and "open source" generate a lot of feelings. If you prefer to call this project proprietary because of the way it's licensed, so be it. This is "a proprietary project with freely available source code that can be used without restriction for non-commercial purposes that engages with its community of users and accepts and encourages their contributions to source code, documentation, and support when they feel it's in their best interest to do so". Call that model whatever you want.

## Why not use a copyleft license instead?

A copyleft license places restrictions on consumers that enforce the "openness" of the software. This is great if your goal is to further the reach of the openness but does nothing to address the sustainability problem. It also places these restrictions indiscriminately - it doesn't matter if you're using the project for commercial or non-commercial uses. When tied to a dual-licensing model in which a less restrictive private license can be purchased, the incentive for doing so is connected to an assumption that licensees have an interest in using the software in a less open environment such as closed-source.

Rather than tie funding to whether or not the consumer wants to also make their code open, it makes more sense to me to tie _paying_ money to _getting_ money. In other words, _if you're generating revenue in part because of the work done on this project, then you should share some of that with the person who did the work you're using to generate the revenue_.

There's also a practical matter in that copyleft licenses only work well with libraries because the license deals with what happens to code that consumes or uses the copyleft-licensed software. Statiq is an application so it's not generally used by or integrated with other code, making the provisions of a copyleft license not applicable.

## I want to contribute, why do you make me sign a CLA?

When an open source project uses a non-permissive license the question of how to deal with contributions comes up. A contributor license agreement (CLA) clarifies what will happen to the contributed code and requires the contributor to agree to it, waiving some of their own copyrights in the process. It's unfortunate and does add a burden to the contribution process but it's necessary to ensure the entire project stays properly licensed and contributions can be licensed under the terms of the Prosperity License and Private License. [Statiq's Contributor Agreement](https://gist.github.com/daveaglick/c7cccacdf7f3d57d05462a64d578d0a5) is designed to be as simple as possible while granting the broadest rights to the project under a non-exlusive agreement (it was developed using [Contributor Agreements](http://contributoragreements.org/) and then slightly modified).

I hope that you understand why a CLA is needed and that it doesn't keep you from contributing. That said, you need to decide if you derive enough value from submitting a contribution (such as adding support for a feature you need) to agree to the terms of the CLA. If not then I would caution you not to undertake performing work on the project.

## What about the contributions back when Statiq was Wyam?

Wyam was licensed under a permissive MIT license and so all contributions prior to the release of Statiq would have also fallen under that license. To put it another way, the MIT licensing of the project meant that anyone, including the project itself, could come along, take the code, and do whatever they wanted with it including re-licensing or charging for it.