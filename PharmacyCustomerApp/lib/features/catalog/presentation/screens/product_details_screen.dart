import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../cart/presentation/providers/cart_providers.dart';
import '../../domain/entities/product.dart';

class ProductDetailsScreen extends ConsumerStatefulWidget {
  const ProductDetailsScreen({required this.product, super.key});
  final Product product;

  @override
  ConsumerState<ProductDetailsScreen> createState() =>
      _ProductDetailsScreenState();
}

class _ProductDetailsScreenState extends ConsumerState<ProductDetailsScreen> {
  bool _adding = false;

  Future<void> _addToCart() async {
    setState(() => _adding = true);
    try {
      await ref.read(cartActionsProvider).add(widget.product.id);
      ref.invalidate(cartProvider);
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(const SnackBar(content: Text('Added to your cart.')));
      }
    } catch (error) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text(error.toString())));
      }
    } finally {
      if (mounted) setState(() => _adding = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final product = widget.product;
    final colors = Theme.of(context).colorScheme;
    return Scaffold(
      appBar: AppBar(title: const Text('Product details')),
      body: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          Container(
            height: 240,
            decoration: BoxDecoration(
              color: colors.primaryContainer.withValues(alpha: .45),
              borderRadius: BorderRadius.circular(24),
            ),
            alignment: Alignment.center,
            child: Icon(
              Icons.medication_outlined,
              size: 96,
              color: colors.primary,
            ),
          ),
          const SizedBox(height: 22),
          Text(
            product.name,
            style: Theme.of(
              context,
            ).textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.bold),
          ),
          if (product.genericName?.isNotEmpty == true) ...[
            const SizedBox(height: 4),
            Text(
              product.genericName!,
              style: Theme.of(context).textTheme.titleMedium,
            ),
          ],
          const SizedBox(height: 12),
          Text(
            product.price.toStringAsFixed(2),
            style: Theme.of(context).textTheme.headlineSmall?.copyWith(
              color: colors.primary,
              fontWeight: FontWeight.w700,
            ),
          ),
          if (product.requiresPrescription) ...[
            const SizedBox(height: 16),
            const Card(
              child: ListTile(
                leading: Icon(Icons.info_outline),
                title: Text('Prescription required'),
                subtitle: Text('This item requires a prescription.'),
              ),
            ),
          ],
          const SizedBox(height: 28),
          FilledButton.icon(
            onPressed: _adding ? null : _addToCart,
            icon: _adding
                ? const SizedBox(
                    width: 18,
                    height: 18,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  )
                : const Icon(Icons.add_shopping_cart),
            label: const Text('Add to cart'),
          ),
        ],
      ),
    );
  }
}
