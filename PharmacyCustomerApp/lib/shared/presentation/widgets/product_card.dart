import 'package:flutter/material.dart';

import '../../../features/catalog/domain/entities/product.dart';

class ProductCard extends StatelessWidget {
  const ProductCard({required this.product, required this.onTap, super.key});
  final Product product;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final colors = Theme.of(context).colorScheme;
    return Card(
      clipBehavior: Clip.antiAlias,
      child: InkWell(
        onTap: onTap,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Expanded(
              child: Stack(
                fit: StackFit.expand,
                children: [
                  DecoratedBox(
                    decoration: BoxDecoration(
                      gradient: LinearGradient(
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                        colors: [colors.primaryContainer.withValues(alpha: .8), const Color(0xFFF6FAF8)],
                      ),
                    ),
                    child: Icon(Icons.medication_outlined, size: 48, color: colors.primary.withValues(alpha: .8)),
                  ),
                  if (product.requiresPrescription)
                    const Positioned(top: 9, left: 9, child: Chip(visualDensity: VisualDensity.compact, label: Text('Rx'), padding: EdgeInsets.zero)),
                ],
              ),
            ),
            Padding(
              padding: const EdgeInsets.all(12),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    product.name,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                    style: Theme.of(context).textTheme.titleSmall,
                  ),
                  if (product.genericName?.isNotEmpty == true) ...[
                    const SizedBox(height: 3),
                    Text(
                      product.genericName!,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: Theme.of(context).textTheme.bodySmall,
                    ),
                  ],
                  const SizedBox(height: 7),
                  Text(
                    product.price.toStringAsFixed(2),
                    style: Theme.of(context).textTheme.titleMedium?.copyWith(
                      color: colors.primary,
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                  if (product.requiresPrescription)
                    Text(
                      'Prescription required',
                      style: Theme.of(
                        context,
                      ).textTheme.labelSmall?.copyWith(color: colors.error),
                    ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
