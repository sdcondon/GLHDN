//https://raw.githubusercontent.com/nothings/stb/master/stb_rect_pack.h


// stb_rect_pack.h - v0.11 - public domain - rectangle packing
// Sean Barrett 2014
//
// Useful for e.g. packing rectangular textures into an atlas.
// Does not do rotation.
//
// Not necessarily the awesomest packing method, but better than
// the totally naive one in stb_truetype (which is primarily what
// this is meant to replace).
//
// Has only had a few tests run, may have issues.
//
// More docs to come.
//
// No memory allocations; uses qsort() and assert() from stdlib.
// Can override those by defining STBRP_SORT and STBRP_ASSERT.
//
// This library currently uses the Skyline Bottom-Left algorithm.
//
// Please note: better rectangle packers are welcome! Please
// implement them to the same API, but with a different init
// function.
//
// Credits
//
//  Library
//    Sean Barrett
//  Minor features
//    Martins Mozeiko
//    github:IntellectualKitty
//    
//  Bugfixes / warning fixes
//    Jeremy Jaussaud
//
// Version history:
//
//     0.11  (2017-03-03)  return packing success/fail result
//     0.10  (2016-10-25)  remove cast-away-const to avoid warnings
//     0.09  (2016-08-27)  fix compiler warnings
//     0.08  (2015-09-13)  really fix bug with empty rects (w=0 or h=0)
//     0.07  (2015-09-13)  fix bug with empty rects (w=0 or h=0)
//     0.06  (2015-04-15)  added STBRP_SORT to allow replacing qsort
//     0.05:  added STBRP_ASSERT to allow replacing assert
//     0.04:  fixed minor bug in STBRP_LARGE_RECTS support
//     0.01:  initial release
//
// LICENSE
//
//   See end of file for license information.
namespace NanoGL.Stb.TrueType
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    class stb_rect_pack
    {
        //#if STBRP_LARGE_RECTS
        //typedef int stbrp_coord;
        //#else
        //typedef unsigned short stbrp_coord;
        //#endif

        private static readonly Action<bool> STBRP_ASSERT = b => Debug.Assert(b);

        // Initialize a rectangle packer to:
        //    pack a rectangle that is 'width' by 'height' in dimensions
        //    using temporary storage provided by the array 'nodes', which is 'num_nodes' long
        //
        // You must call this function every time you start packing into a new target.
        //
        // There is no "shutdown" function. The 'nodes' memory must stay valid for
        // the following stbrp_pack_rects() call (or calls), but can be freed after
        // the call (or calls) finish.
        //
        // Note: to guarantee best results, either:
        //       1. make sure 'num_nodes' >= 'width'
        //   or  2. call stbrp_allow_out_of_mem() defined below with 'allow_out_of_mem = 1'
        //
        // If you don't do either of the above things, widths will be quantized to multiples
        // of small integers to guarantee the algorithm doesn't run out of temporary storage.
        //
        // If you do #2, then the non-quantized algorithm will be used, but the algorithm
        // may run out of temporary storage and be unable to pack some rectangles.
        public static void stbrp_init_target(ref stbrp_context context, int width, int height, stbrp_node[] nodes)
        {
#if !STBRP_LARGE_RECTS
            STBRP_ASSERT(width <= 0xffff && height <= 0xffff);
#endif
            int i;
            for (i = 0; i < nodes.Length - 1; ++i)
            {
                nodes[i].next = nodes[i + 1];
            }
            nodes[i].next = null;

            context.init_mode = STBRP__INIT.skyline;
            context.heuristic = STBRP_HEURISTIC.Skyline_default;
            context.free_head = nodes[0];
            context.active_head = context.extra[0];
            context.width = width;
            context.height = height;
            context.num_nodes = nodes.Length;
            stbrp_setup_allow_out_of_mem(ref context, false);

            // node 0 is the full width
            context.extra[0].x = 0;
            context.extra[0].y = 0;
            context.extra[0].next = context.extra[1];

            // node 1 is the sentinel (lets us not store width explicitly)
            context.extra[1].x = width;
#if STBRP_LARGE_RECTS
            context.extra[1].y = (1 << 30);
#else
            context.extra[1].y = 65535;
#endif
            context.extra[1].next = null;
        }

        // Assign packed locations to rectangles. The rectangles are of type
        // 'stbrp_rect' defined below, stored in the array 'rects', and there
        // are 'num_rects' many of them.
        //
        // Rectangles which are successfully packed have the 'was_packed' flag
        // set to a non-zero value and 'x' and 'y' store the minimum location
        // on each axis (i.e. bottom-left in cartesian coordinates, top-left
        // if you imagine y increasing downwards). Rectangles which do not fit
        // have the 'was_packed' flag set to 0.
        //
        // You should not try to access the 'rects' array from another thread
        // while this function is running, as the function temporarily reorders
        // the array while it executes.
        //
        // To pack into another rectangle, you need to call stbrp_init_target
        // again. To continue packing into the same rectangle, you can call
        // this function again. Calling this multiple times with multiple rect
        // arrays will probably produce worse packing results than calling it
        // a single time with the full rectangle array, but the option is
        // available.
        //
        // The function returns 1 if all of the rectangles were successfully
        // packed and 0 otherwise.
        public static int stbrp_pack_rects(ref stbrp_context context, stbrp_rect[] rects)
        {
            // we use the 'was_packed' field internally to allow sorting/unsorting
            for (int i = 0; i < rects.Length; ++i)
            {
                rects[i].was_packed = i;
#if !STBRP_LARGE_RECTS
                STBRP_ASSERT(rects[i].w <= 0xffff && rects[i].h <= 0xffff);
#endif
            }

            // sort according to heuristic
            Array.Sort(rects, rect_height_compare);

            for (int i = 0; i < rects.Length; ++i)
            {
                if (rects[i].w == 0 || rects[i].h == 0)
                {
                    rects[i].x = rects[i].y = 0;  // empty rect needs no space
                }
                else
                {
                    stbrp__findresult fr = stbrp__skyline_pack_rectangle(ref context, rects[i].w, rects[i].h);
                    if (fr.prev_link != null)
                    {
                        rects[i].x = fr.x;
                        rects[i].y = fr.y;
                    }
                    else
                    {
                        rects[i].x = rects[i].y = int.MaxValue;
                    }
                }
            }

            // unsort
            Array.Sort(rects, rect_original_order);

            // set was_packed flags and all_rects_packed status
            int all_rects_packed = 1;
            for (int i = 0; i < rects.Length; ++i)
            {
                rects[i].was_packed = !(rects[i].x == int.MaxValue && rects[i].y == int.MaxValue) ? 1 : 0;
                if (rects[i].was_packed != 0)
                {
                    all_rects_packed = 0;
                }
            }

            return all_rects_packed;
        }

        // 16 bytes, nominally
        public struct stbrp_rect
        {
            // reserved for your use:
            public int id;

            // input:
            public int w, h;

            // output:
            public int x, y;
            public int was_packed;  // non-zero if valid packing
        }; 

        // Optionally call this function after init but before doing any packing to
        // change the handling of the out-of-temp-memory scenario, described above.
        // If you call init again, this will be reset to the default (false).
        public static void stbrp_setup_allow_out_of_mem(ref stbrp_context context, bool allow_out_of_mem)
        {
            if (allow_out_of_mem)
            {
                // if it's ok to run out of memory, then don't bother aligning them;
                // this gives better packing, but may fail due to OOM (even though
                // the rectangles easily fit). @TODO a smarter approach would be to only
                // quantize once we've hit OOM, then we could get rid of this parameter.
                context.align = 1;
            }
            else
            {
                // if it's not ok to run out of memory, then quantize the widths
                // so that num_nodes is always enough nodes.
                // I.e. num_nodes * align >= width
                //                  align >= width / num_nodes
                //                  align = ceil(width/num_nodes)
                context.align = (context.width + context.num_nodes - 1) / context.num_nodes;
            }
        }

        // Optionally select which packing heuristic the library should use. Different
        // heuristics will produce better/worse results for different data sets.
        // If you call init again, this will be reset to the default.
        public static void stbrp_setup_heuristic(ref stbrp_context context, STBRP_HEURISTIC heuristic)
        {
            switch (context.init_mode)
            {
                case STBRP__INIT.skyline:
                    STBRP_ASSERT(heuristic == STBRP_HEURISTIC.Skyline_BL_sortHeight || heuristic == STBRP_HEURISTIC.Skyline_BF_sortHeight);
                    context.heuristic = heuristic;
                    break;
                default:
                    STBRP_ASSERT(false);
                    break;
            }
        }

        public enum STBRP_HEURISTIC
        {
            Skyline_default = 0,
            Skyline_BL_sortHeight = Skyline_default,
            Skyline_BF_sortHeight
        }

        //////////////////////////////////////////////////////////////////////////////
        // the details of the following structures don't matter to you, but they must
        // be visible so you can handle the memory allocations for them

        public class stbrp_node
        {
            public int x, y;
            public stbrp_node next;
        };

        public struct stbrp_context
        {
            public int width;
            public int height;
            public int align;
            public STBRP__INIT init_mode;
            public STBRP_HEURISTIC heuristic;
            public int num_nodes;
            public stbrp_node active_head;
            public stbrp_node free_head;
            public stbrp_node[] extra = new stbrp_node[2]; // we allocate two extra nodes so optimal user-node-count is 'width' not 'width+2'
        };

        private enum STBRP__INIT
        {
            skyline = 1
        }

        // find minimum y position if it starts at x1
        static int stbrp__skyline_find_min_y(ref stbrp_context c, stbrp_node first, int x0, int width, ref int pwaste)
        {
            stbrp_node node = first;
            int x1 = x0 + width;
            int min_y, visited_width, waste_area;

            // STBRP__NOTUSED(c);
            STBRP_ASSERT(first.x <= x0);
#if false
            // skip in case we're past the node
            while (node->next->x <= x0)
            ++node;
#else
            STBRP_ASSERT(node.next.x > x0); // we ended up handling this in the caller for efficiency
#endif

            STBRP_ASSERT(node.x <= x0);

            min_y = 0;
            waste_area = 0;
            visited_width = 0;
            while (node.x < x1)
            {
                if (node.y > min_y)
                {
                    // raise min_y higher.
                    // we've accounted for all waste up to min_y,
                    // but we'll now add more waste for everything we've visted
                    waste_area += visited_width * (node.y - min_y);
                    min_y = node.y;
                    // the first time through, visited_width might be reduced
                    if (node.x < x0)
                    {
                        visited_width += node.next.x - x0;
                    }
                    else
                    {
                        visited_width += node.next.x - node.x;
                    }
                }
                else
                {
                    // add waste area
                    int under_width = node.next.x - node.x;
                    if (under_width + visited_width > width)
                        under_width = width - visited_width;
                    waste_area += under_width * (min_y - node.y);
                    visited_width += under_width;
                }
                node = node.next;
            }

            pwaste = waste_area;
            return min_y;
        }

        struct stbrp__findresult
        {
            public int x, y;
            public stbrp_node** prev_link;
        }

        static stbrp__findresult stbrp__skyline_find_best_pos(ref stbrp_context c, int width, int height)
        {
            int best_waste = (1 << 30), best_x, best_y = (1 << 30);
            stbrp__findresult fr;
            stbrp_node *tail, **best = NULL;

            // align to multiple of c.align
            width = (width + c.align - 1);
            width -= width % c.align;
            STBRP_ASSERT(width % c.align == 0);

            var node = c.active_head;
            stbrp_node** prev = &c->active_head;
            while (node.x + width <= c.width)
            {
                int y, waste;
                y = stbrp__skyline_find_min_y(c, node, node.x, width, ref waste);
                if (c.heuristic == STBRP_HEURISTIC.Skyline_BL_sortHeight)
                {
                    // actually just want to test BL
                    // bottom left
                    if (y < best_y)
                    {
                        best_y = y;
                        best = prev;
                    }
                }
                else
                {
                    // best-fit
                    if (y + height <= c.height)
                    {
                        // can only use it if it first vertically
                        if (y < best_y || (y == best_y && waste < best_waste))
                        {
                            best_y = y;
                            best_waste = waste;
                            best = prev;
                        }
                    }
                }

                prev = &node.next;
                node = node.next;
            }

            best_x = (best == null) ? 0 : (*best)->x;

            // if doing best-fit (BF), we also have to try aligning right edge to each node position
            //
            // e.g, if fitting
            //
            //     ____________________
            //    |____________________|
            //
            //            into
            //
            //   |                         |
            //   |             ____________|
            //   |____________|
            //
            // then right-aligned reduces waste, but bottom-left BL is always chooses left-aligned
            //
            // This makes BF take about 2x the time

            if (c.heuristic == STBRP_HEURISTIC.Skyline_BF_sortHeight)
            {
                tail = c->active_head;
                node = c->active_head;
                prev = &c->active_head;
                // find first node that's admissible
                while (tail->x < width)
                    tail = tail->next;
                while (tail)
                {
                    int xpos = tail->x - width;
                    int y, waste;
                    STBRP_ASSERT(xpos >= 0);
                    // find the left position that matches this
                    while (node->next->x <= xpos)
                    {
                        prev = &node->next;
                        node = node->next;
                    }
                    STBRP_ASSERT(node->next->x > xpos && node->x <= xpos);
                    y = stbrp__skyline_find_min_y(c, node, xpos, width, &waste);
                    if (y + height < c->height)
                    {
                        if (y <= best_y)
                        {
                            if (y < best_y || waste < best_waste || (waste == best_waste && xpos < best_x))
                            {
                                best_x = xpos;
                                STBRP_ASSERT(y <= best_y);
                                best_y = y;
                                best_waste = waste;
                                best = prev;
                            }
                        }
                    }
                    tail = tail->next;
                }
            }

            fr.prev_link = best;
            fr.x = best_x;
            fr.y = best_y;
            return fr;
        }

        static stbrp__findresult stbrp__skyline_pack_rectangle(ref stbrp_context context, int width, int height)
        {
            // find best position according to heuristic
            stbrp__findresult res = stbrp__skyline_find_best_pos(context, width, height);
            stbrp_node node, cur;

            // bail if:
            //    1. it failed
            //    2. the best node doesn't fit (we don't always check this)
            //    3. we're out of memory
            if (res.prev_link == null || res.y + height > context.height || context.free_head == null)
            {
                res.prev_link = null;
                return res;
            }

            // on success, create new node
            node = context.free_head;
            node.x = res.x;
            node.y = res.y + height;

            context.free_head = node.next;

            // insert the new node into the right starting point, and
            // let 'cur' point to the remaining nodes needing to be
            // stiched back in

            cur = *res.prev_link;
            if (cur->x < res.x)
            {
                // preserve the existing one, so start testing with the next one
                stbrp_node* next = cur->next;
                cur->next = node;
                cur = next;
            }
            else
            {
                *res.prev_link = node;
            }

            // from here, traverse cur and free the nodes, until we get to one
            // that shouldn't be freed
            while (cur->next && cur->next->x <= res.x + width)
            {
                stbrp_node* next = cur->next;
                // move the current node to the free list
                cur->next = context->free_head;
                context->free_head = cur;
                cur = next;
            }

            // stitch the list back in
            node->next = cur;

            if (cur->x < res.x + width)
                cur->x = (stbrp_coord)(res.x + width);

# ifdef _DEBUG
            cur = context->active_head;
            while (cur->x < context->width)
            {
                STBRP_ASSERT(cur->x < cur->next->x);
                cur = cur->next;
            }
            STBRP_ASSERT(cur->next == NULL);

            {
                stbrp_node* L1 = NULL, *L2 = NULL;
                int count = 0;
                cur = context->active_head;
                while (cur)
                {
                    L1 = cur;
                    cur = cur->next;
                    ++count;
                }
                cur = context->free_head;
                while (cur)
                {
                    L2 = cur;
                    cur = cur->next;
                    ++count;
                }
                STBRP_ASSERT(count == context->num_nodes + 2);
            }
#endif

            return res;
        }

        private static int rect_height_compare(stbrp_rect p, stbrp_rect q)
        {
            if (p.h > q.h)
                return -1;
            if (p.h < q.h)
                return  1;
            return (p.w > q.w) ? -1 : (p.w < q.w ? 1 : 0);
        }

        private static int rect_original_order(stbrp_rect p, stbrp_rect q)
        {
           return (p.was_packed < q.was_packed) ? -1 : (p.was_packed > q.was_packed ? 1 : 0);
        }

//# ifdef STBRP_LARGE_RECTS
//#define STBRP__MAXVAL  0xffffffff
//#else
//#define STBRP__MAXVAL  0xffff
//#endif
    }
}