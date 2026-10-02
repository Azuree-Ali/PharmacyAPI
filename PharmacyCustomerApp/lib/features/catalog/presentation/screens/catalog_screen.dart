import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../shared/presentation/widgets/product_card.dart';
import '../../domain/entities/product.dart';
import '../providers/catalog_providers.dart';
import 'product_details_screen.dart';

class CatalogScreen extends ConsumerStatefulWidget {
  const CatalogScreen({super.key});
  @override
  ConsumerState<CatalogScreen> createState() => _CatalogScreenState();
}

class _CatalogScreenState extends ConsumerState<CatalogScreen> {
  String _query = '';
  int? _categoryId;
  @override
  Widget build(BuildContext context) {
    final ref = this.ref;
    final products = ref.watch(productsProvider);
    return Scaffold(
      appBar: AppBar(
        title: const Text('Shop'),
        actions: [
          IconButton(
            tooltip: 'Refresh products',
            onPressed: () => ref.invalidate(productsProvider),
            icon: const Icon(Icons.refresh),
          ),
        ],
      ),
      body: products.when(
        loading: () => const Center(child: CircularProgressIndicator()),
        error: (error, stack) => Center(
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                const Icon(Icons.cloud_off_outlined, size: 44),
                const SizedBox(height: 12),
                Text(error.toString(), textAlign: TextAlign.center),
                const SizedBox(height: 12),
                OutlinedButton(
                  onPressed: () => ref.invalidate(productsProvider),
                  child: const Text('Try again'),
                ),
              ],
            ),
          ),
        ),
        data: (items) {
          final term = _query.trim().toLowerCase();
          final visible = items
              .where(
                (product) =>
                    (_categoryId == null ||
                        product.categoryId == _categoryId) &&
                    (term.isEmpty ||
                        product.name.toLowerCase().contains(term) ||
                        (product.genericName?.toLowerCase().contains(term) ??
                            false)),
              )
              .toList();
          return Column(
            children: [
              Padding(
                padding: const EdgeInsets.fromLTRB(16, 4, 16, 10),
                child: TextField(
                  onChanged: (value) => setState(() => _query = value),
                  decoration: InputDecoration(
                    hintText: 'Search medicines',
                    prefixIcon: const Icon(Icons.search),
                    suffixIcon: _query.isEmpty
                        ? null
                        : IconButton(
                            onPressed: () => setState(() => _query = ''),
                            icon: const Icon(Icons.close),
                          ),
                  ),
                ),
              ),
              ref
                  .watch(customerHomeProvider)
                  .when(
                    data: (home) => SizedBox(
                      height: 48,
                      child: ListView(
                        scrollDirection: Axis.horizontal,
                        padding: const EdgeInsets.symmetric(horizontal: 16),
                        children: [
                          Padding(
                            padding: const EdgeInsets.only(right: 8),
                            child: ChoiceChip(
                              label: const Text('All'),
                              selected: _categoryId == null,
                              onSelected: (_) =>
                                  setState(() => _categoryId = null),
                            ),
                          ),
                          ...home.categories.map(
                            (category) => Padding(
                              padding: const EdgeInsets.only(right: 8),
                              child: ChoiceChip(
                                label: Text(category.name),
                                selected: _categoryId == category.id,
                                onSelected: (_) =>
                                    setState(() => _categoryId = category.id),
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                    error: (_, _) => const SizedBox(height: 4),
                    loading: () => const SizedBox(height: 4),
                  ),
              Expanded(
                child: visible.isEmpty
                    ? Center(
                        child: Padding(
                          padding: const EdgeInsets.all(24),
                          child: Column(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              Icon(
                                Icons.search_off,
                                size: 48,
                                color: Theme.of(context).colorScheme.primary,
                              ),
                              const SizedBox(height: 12),
                              Text(
                                items.isEmpty
                                    ? 'No products are available right now.'
                                    : 'No products match your search.',
                                textAlign: TextAlign.center,
                              ),
                            ],
                          ),
                        ),
                      )
                    : _ProductGrid(products: visible),
              ),
            ],
          );
        },
      ),
    );
  }
}

class _ProductGrid extends StatelessWidget {
  const _ProductGrid({required this.products});
  final List<Product> products;

  @override
  Widget build(BuildContext context) => LayoutBuilder(
    builder: (context, bounds) {
      final columns = (bounds.maxWidth / 190).floor().clamp(2, 5).toInt();
      return GridView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: products.length,
        gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
          crossAxisCount: columns,
          mainAxisSpacing: 14,
          crossAxisSpacing: 14,
          childAspectRatio: columns == 2 ? .72 : .78,
        ),
        itemBuilder: (context, index) => ProductCard(
          product: products[index],
          onTap: () => Navigator.of(context).push(
            MaterialPageRoute<void>(
              builder: (_) => ProductDetailsScreen(product: products[index]),
            ),
          ),
        ),
      );
    },
  );
}
